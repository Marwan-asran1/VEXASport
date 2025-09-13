using Microsoft.AspNetCore.Mvc;
using VEXA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;

namespace VEXA.Controllers
{
    [Authorize(Policy = "RequireAdmin")] // Only admins can access this controller
    public class AdminController(AppDbContext context, ILogger<AdminController> logger, IPasswordHasher<User> passwordHasher) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<AdminController> _logger = logger;
        private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

        private void WriteAudit(string action, string entityType, int entityId, object? beforeObj, object? afterObj)
        {
            try
            {
                var adminId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var adminEmail = User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var adminName = User?.Identity?.Name;

                var audit = new AdminAuditLog
                {
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    AdminUserId = adminId,
                    AdminEmail = adminEmail,
                    AdminName = adminName,
                    OccurredAtUtc = DateTime.UtcNow,
                    BeforeJson = beforeObj == null ? null : System.Text.Json.JsonSerializer.Serialize(beforeObj),
                    AfterJson = afterObj == null ? null : System.Text.Json.JsonSerializer.Serialize(afterObj)
                };
                _context.AdminAuditLogs.Add(audit);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write admin audit log {Action} {EntityType} {EntityId}", action, entityType, entityId);
            }
        }

        public IActionResult Admin()
        {
            // Calculate dynamic earnings
            var earningsData = CalculateEarnings();
            return View("AdminHome", earningsData);
        }

        public IActionResult Products(int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var products = _context.Products
                .AsNoTracking()
                .Include(p => p.Variants)
                .OrderByDescending(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return View("Admin_allproducts", products);
        }

        public IActionResult Users(int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            List<User> users = _context.Users
                .AsNoTracking()
                .OrderByDescending(u => u.RegistrationDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return View("Admin_users", users);
        }

        public IActionResult Orders(int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            List<Order> orders = _context.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return View("Admin_Orders", orders);
        }

        [HttpGet]
        public IActionResult GetOrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            var orderDetails = new
            {
                success = true,
                order = new
                {
                    id = order.Id,
                    orderDate = order.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                    customerName = order.User?.Name ?? "N/A",
                    customerEmail = order.User?.Email ?? "N/A",
                    shippingAddress = order.ShippingAddress,
                    billingAddress = order.BillingAddress,
                    contactPhone = order.ContactPhone,
                    status = order.Status.ToString(),
                    method = order.Method.ToString(),
                    orderTotal = order.OrderTotal.ToString("F2"),
                    deliveredDate = order.DeliveredDate?.ToString("yyyy-MM-dd") ?? "N/A",
                    items = order.OrderItems.Select(oi => new
                    {
                        productName = oi.Product?.Name ?? "Unknown Product",
                        productGender = oi.Product?.Gender.ToString() ?? "N/A",
                        productType = oi.Product?.ProductType.ToString() ?? "N/A",
                        size = oi.Size.ToString(),
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice.ToString("F2"),
                        totalPrice = oi.TotalPrice.ToString("F2")
                    }).ToList()
                }
            };

            return Json(orderDetails);
        }

        // User CRUD Operations
        public class CreateUserDto
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string PhoneNumber { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public CustomerGender Gender { get; set; }
            public int UserRole { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser([FromForm] CreateUserDto dto)
        {
            // Handle empty strings and convert to proper types
            if (string.IsNullOrWhiteSpace(dto.Name))
                ModelState.AddModelError("Name", "Name is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                ModelState.AddModelError("Email", "Email is required");

            // Password optional for admin creation; a strong temp will be generated if empty

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                ModelState.AddModelError("PhoneNumber", "Phone number is required");

            if (string.IsNullOrWhiteSpace(dto.Address))
                ModelState.AddModelError("Address", "Address is required");

            // Gender validation is handled by the enum default value

            // Ensure UserRole is valid
            if (dto.UserRole < 0 || dto.UserRole > 1)
                ModelState.AddModelError("UserRole", "Invalid role selected");

            // Pre-check email uniqueness
            if (!string.IsNullOrWhiteSpace(dto.Email) && _context.Users.Any(u => u.Email == dto.Email.Trim()))
            {
                ModelState.AddModelError("Email", "Email already registered");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Validation failed: {string.Join(", ", errors)}";
                return RedirectToAction("Users");
            }

            try
            {
                // Set default values
                var user = new User
                {
                    Name = dto.Name.Trim(),
                    Email = dto.Email.Trim(),
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Address = dto.Address.Trim(),
                    Gender = dto.Gender,
                    UserRole = dto.UserRole,
                    RegistrationDate = DateTime.UtcNow
                };
                // Hash password (generate if not provided)
                var passwordToUse = string.IsNullOrWhiteSpace(dto.Password) ? GenerateTemporaryPassword() : dto.Password;
                user.Password = _passwordHasher.HashPassword(user, passwordToUse);

                _context.Users.Add(user);
                _context.SaveChanges();
                WriteAudit("CreateUser", nameof(User), user.Id, null, new { user.Id, user.Name, user.Email, user.UserRole });
                TempData["Success"] = "User created successfully!";
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user");
                TempData["Error"] = $"Failed to create user: {ex.Message}";
                return RedirectToAction("Users");
            }
        }

        private static string GenerateTemporaryPassword()
        {
            const string allowed = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@$?*";
            var bytes = new byte[12];
            RandomNumberGenerator.Fill(bytes);
            var chars = new char[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                chars[i] = allowed[bytes[i] % allowed.Length];
            }
            return new string(chars);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateUser(UserUpdateModel userUpdate)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Validation failed: {string.Join(", ", errors)}";
                return RedirectToAction("Users");
            }

            try
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Id == userUpdate.Id);
                if (existingUser != null)
                {
                    var before = new { existingUser.Name, existingUser.Email, existingUser.UserRole, existingUser.Gender };
                    // Update only the allowed fields, preserve password
                    existingUser.Name = userUpdate.Name.Trim();
                    existingUser.Email = userUpdate.Email.Trim();
                    existingUser.PhoneNumber = userUpdate.PhoneNumber.Trim();
                    existingUser.Address = userUpdate.Address.Trim();
                    existingUser.UserRole = userUpdate.UserRole;
                    existingUser.Gender = userUpdate.Gender;
                    // Password is intentionally not updated

                    // Concurrency check
                    if (userUpdate.RowVersion != null)
                    {
                        _context.Entry(existingUser).Property(x => x.RowVersion).OriginalValue = userUpdate.RowVersion;
                    }

                    try
                    {
                        _context.SaveChanges();
                        // If current logged-in user's own role changed, sign them out to refresh claims next request
                        var currentUserId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        if (!string.IsNullOrEmpty(currentUserId) && currentUserId == existingUser.Id.ToString())
                        {
                            HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme).Wait();
                        }
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        TempData["Error"] = "The user was modified by another admin. Please reload and try again.";
                        return RedirectToAction("Users");
                    }
                    WriteAudit("UpdateUser", nameof(User), existingUser.Id, before, new { existingUser.Name, existingUser.Email, existingUser.UserRole, existingUser.Gender });
                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("Users");
                }
                else
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Users");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user {UserId}", userUpdate.Id);
                TempData["Error"] = $"Failed to update user: {ex.Message}";
                return RedirectToAction("Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                var user = _context.Users.Find(id);
                if (user != null)
                {
                    var before = new { user.Name, user.Email, user.UserRole, user.Gender };
                    _context.Users.Remove(user);
                    _context.SaveChanges();
                    WriteAudit("DeleteUser", nameof(User), id, before, null);
                    TempData["Success"] = "User deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "User not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user {UserId}", id);
                TempData["Error"] = $"Failed to delete user: {ex.Message}";
            }
            return RedirectToAction("Users");
        }

        [HttpGet]
        public IActionResult GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
                return NotFound();

            // Return safe projection without sensitive fields (e.g., Password)
            var safe = new {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                address = user.Address,
                gender = user.Gender.ToString(),
                userRole = user.UserRole,
                registrationDate = user.RegistrationDate,
                rowVersion = user.RowVersion != null ? Convert.ToBase64String(user.RowVersion) : null
            };
            return Json(safe);
        }

        // Product CRUD Operations
        public class CreateProductDto
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string? ImageUrl { get; set; }
            public Gender Gender { get; set; }
            public ProductType ProductType { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct([FromForm] CreateProductDto dto)
        {
            // Debug: Log the received product data
            _logger.LogInformation("Creating product: Name={Name}, Gender={Gender}, ProductType={ProductType}", 
                dto.Name, dto.Gender, dto.ProductType);

            // Handle empty strings and convert to proper types
            if (string.IsNullOrWhiteSpace(dto.Name))
                ModelState.AddModelError("Name", "Product name is required");

            if (string.IsNullOrWhiteSpace(dto.Description))
                ModelState.AddModelError("Description", "Product description is required");

            if (dto.Price <= 0)
                ModelState.AddModelError("Price", "Price must be greater than 0");

            // Stock is managed via variants now; no per-size stock validation here

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["Error"] = $"Validation failed: {string.Join(", ", errors)}";
                _logger.LogWarning("Product creation validation failed: {Errors}", string.Join(", ", errors));
                return RedirectToAction("Products");
            }

            try
            {
                // Set default values
                var product = new Product
                {
                    Name = dto.Name.Trim(),
                    Description = dto.Description.Trim(),
                    Price = dto.Price,
                    ImageUrl = dto.ImageUrl,
                    Gender = dto.Gender,
                    ProductType = dto.ProductType,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Products.Add(product);
                _context.SaveChanges();
                WriteAudit("CreateProduct", nameof(Product), product.Id, null, new { product.Id, product.Name, product.Price, product.Gender, product.ProductType });
                
                // Create variants for all sizes with default stock
                var sizes = new[] { ClothingSize.S, ClothingSize.M, ClothingSize.L };
                foreach (var size in sizes)
                {
                    var variant = new ProductVariant
                    {
                        ProductId = product.Id,
                        Size = size,
                        StockQuantity = 10 // Default stock quantity
                    };
                    _context.ProductVariants.Add(variant);
                    _logger.LogInformation("Created variant for product {ProductId}: Size={Size}, Stock={Stock}", 
                        product.Id, size, variant.StockQuantity);
                }
                
                _context.SaveChanges();
                _logger.LogInformation("Product {ProductId} created with {Count} variants", product.Id, sizes.Length);
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create product");
                TempData["Error"] = $"Failed to create product: {ex.Message}";
                return RedirectToAction("Products");
            }
        }

        public class UpdateProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string? ImageUrl { get; set; }
            public Gender Gender { get; set; }
            public ProductType ProductType { get; set; }
            public byte[]? RowVersion { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProduct([FromForm] UpdateProductDto dto)
        {
            // Debug logging
            _logger.LogInformation("UpdateProduct called with Id: {ProductId}, Name: {ProductName}", dto.Id, dto.Name);

            // Handle empty strings and convert to proper types
            if (string.IsNullOrWhiteSpace(dto.Name))
                ModelState.AddModelError("Name", "Product name is required");

            if (string.IsNullOrWhiteSpace(dto.Description))
                ModelState.AddModelError("Description", "Product description is required");

            if (dto.Price <= 0)
                ModelState.AddModelError("Price", "Price must be greater than 0");

            // Validate that we have a valid product ID
            if (dto.Id <= 0)
            {
                ModelState.AddModelError("Id", "Invalid product ID");
                _logger.LogWarning("UpdateProduct called with invalid product ID: {ProductId}", dto.Id);
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("UpdateProduct validation failed: {Errors}", string.Join(", ", errors));
                TempData["Error"] = $"Validation failed: {string.Join(", ", errors)}";
                return RedirectToAction("Products");
            }

            try
            {
                var existingProduct = _context.Products.FirstOrDefault(p => p.Id == dto.Id);
                if (existingProduct != null)
                {
                    existingProduct.Name = dto.Name.Trim();
                    existingProduct.Description = dto.Description.Trim();
                    existingProduct.Price = dto.Price;
                    existingProduct.ImageUrl = dto.ImageUrl;
                    existingProduct.Gender = dto.Gender;
                    existingProduct.ProductType = dto.ProductType;
                    // Size is now managed through ProductVariants, not directly on Product

                    if (dto.RowVersion != null)
                    {
                        _context.Entry(existingProduct).Property(x => x.RowVersion).OriginalValue = dto.RowVersion;
                    }

                    try
                    {
                        _context.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        TempData["Error"] = "The product was modified by another user. Please reload and try again.";
                        return RedirectToAction("Products");
                    }
                    WriteAudit("UpdateProduct", nameof(Product), existingProduct.Id, null, new { existingProduct.Id, existingProduct.Name, existingProduct.Price, existingProduct.Gender, existingProduct.ProductType });
                    _logger.LogInformation("Product {ProductId} updated successfully", dto.Id);
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction("Products");
                }
                else
                {
                    _logger.LogWarning("Product with ID {ProductId} not found for update", dto.Id);
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Products");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update product {ProductId}", dto.Id);
                TempData["Error"] = $"Failed to update product: {ex.Message}";
                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                var product = _context.Products
                    .Include(p => p.Variants)
                    .Include(p => p.OrderItems)
                    .FirstOrDefault(p => p.Id == id);
                    
                if (product != null)
                {
                    // Check if product has any order items and log warning
                    if (product.OrderItems != null && product.OrderItems.Any())
                    {
                        _logger.LogWarning("Deleting product {ProductId} ({ProductName}) that has {OrderCount} order items. Order records will be preserved.", 
                            product.Id, product.Name, product.OrderItems.Count);
                    }
                    
                    // Remove all variants first
                    if (product.Variants != null && product.Variants.Any())
                    {
                        _context.ProductVariants.RemoveRange(product.Variants);
                    }
                    
                    // Remove the product (OrderItems will be preserved due to foreign key constraints)
                    var before = new { product.Name, product.Price, product.Gender, product.ProductType };
                    _context.Products.Remove(product);
                    _context.SaveChanges();
                    WriteAudit("DeleteProduct", nameof(Product), id, before, null);
                    
                    TempData["Success"] = "Product deleted successfully! Order records have been preserved.";
                }
                else
                {
                    TempData["Error"] = "Product not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete product {ProductId}", id);
                TempData["Error"] = $"Failed to delete product: {ex.Message}";
            }
            return RedirectToAction("Products");
        }

        [HttpGet]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products
                .Include(p => p.Variants)
                .FirstOrDefault(p => p.Id == id);
            
            if (product == null)
                return NotFound();

            var productData = new
            {
                id = product.Id,
                name = product.Name,
                description = product.Description,
                price = product.Price,
                imageUrl = product.ImageUrl,
                gender = product.Gender.ToString(),
                productType = product.ProductType.ToString(),
                rowVersion = product.RowVersion != null ? Convert.ToBase64String(product.RowVersion) : null,
                variants = product.Variants?.Select(v => new
                {
                    id = v.Id,
                    size = v.Size.ToString(),
                    stockQuantity = v.StockQuantity
                }).OrderBy(v => v.size).ToList()
            };

            return Json(productData);
        }

        // ===================== Product Variants Management =====================
        [HttpGet]
        public IActionResult GetProductVariants(int productId)
        {
            var product = _context.Products.Include(p => p.Variants).FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return NotFound();

            var variants = (product.Variants ?? [])
                .OrderBy(v => v.Size)
                .Select(v => new { v.Id, v.ProductId, Size = v.Size.ToString(), v.StockQuantity })
                .ToList();

            return Json(variants);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddOrUpdateVariant(int productId, string size, int stockQuantity)
        {
            if (productId <= 0 || string.IsNullOrWhiteSpace(size))
                return BadRequest("Invalid variant payload");

            if (!Enum.TryParse<ClothingSize>(size, true, out var parsedSize) || parsedSize == ClothingSize.Unknown)
                return BadRequest("Invalid size value");

            if (stockQuantity < 0)
                return BadRequest("Stock must be non-negative");

            var product = _context.Products.Include(p => p.Variants).FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return NotFound("Product not found");

            var existing = product.Variants?.FirstOrDefault(v => v.Size == parsedSize);
            if (existing == null)
            {
                var v = new ProductVariant
                {
                    ProductId = productId,
                    Size = parsedSize,
                    StockQuantity = stockQuantity
                };
                _context.ProductVariants.Add(v);
            }
            else
            {
                existing.StockQuantity = stockQuantity;
            }

            _context.SaveChanges();
            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteVariant(int id)
        {
            var variant = _context.ProductVariants.Find(id);
            if (variant == null)
                return NotFound();

            _context.ProductVariants.Remove(variant);
            _context.SaveChanges();
            return RedirectToAction("Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProductVariants([FromBody] UpdateProductVariantsRequest request)
        {
            try
            {
                _logger.LogInformation("UpdateProductVariants called with ProductId: {ProductId}, Variants count: {Count}", 
                    request.ProductId, request.Variants?.Count ?? 0);

                var product = _context.Products.Include(p => p.Variants).FirstOrDefault(p => p.Id == request.ProductId);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found for variant update", request.ProductId);
                    return Json(new { success = false, message = "Product not found" });
                }

                // Clear existing variants
                if (product.Variants != null && product.Variants.Any())
                {
                    _logger.LogInformation("Removing {Count} existing variants for product {ProductId}", 
                        product.Variants.Count, request.ProductId);
                    _context.ProductVariants.RemoveRange(product.Variants);
                }

                // Add new variants
                int addedCount = 0;
                foreach (var variant in request.Variants ?? new List<ProductVariantUpdate>())
                {
                    if (variant.StockQuantity >= 0) // Only add variants with valid stock
                    {
                        // Parse the size string to enum
                        if (Enum.TryParse<ClothingSize>(variant.Size, true, out var parsedSize))
                        {
                            var newVariant = new ProductVariant
                            {
                                ProductId = request.ProductId,
                                Size = parsedSize,
                                StockQuantity = variant.StockQuantity
                            };
                            _context.ProductVariants.Add(newVariant);
                            addedCount++;
                            _logger.LogInformation("Added variant: Size={Size}, Stock={Stock}", parsedSize, variant.StockQuantity);
                        }
                        else
                        {
                            _logger.LogWarning("Invalid size value: {Size}", variant.Size);
                        }
                    }
                }

                _context.SaveChanges();
                _logger.LogInformation("Successfully updated {Count} variants for product {ProductId}", addedCount, request.ProductId);
                return Json(new { success = true, message = "Product variants updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update variants for product {ProductId}", request.ProductId);
                return Json(new { success = false, message = $"Failed to update variants: {ex.Message}" });
            }
        }

        // ===================== Earnings Calculation =====================
        private EarningsViewModel CalculateEarnings()
        {
            var currentDate = DateTime.UtcNow;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            var startOfYear = new DateTime(currentDate.Year, 1, 1);
            
            // Calculate monthly earnings (current month)
            var monthlyEarnings = _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == Order.OrderStatus.Confirmed && o.OrderDate >= startOfMonth && o.OrderDate < startOfMonth.AddMonths(1))
                .Sum(o => o.OrderTotal);
            
            // Calculate annual earnings (current year)
            var annualEarnings = _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == Order.OrderStatus.Confirmed && o.OrderDate >= startOfYear && o.OrderDate < startOfYear.AddYears(1))
                .Sum(o => o.OrderTotal);
            
            // Calculate total orders count
            var totalOrders = _context.Orders.AsNoTracking().Count();
            
            // Calculate total users count
            var totalUsers = _context.Users.AsNoTracking().Count();
            
            // Calculate total products count
            var totalProducts = _context.Products.AsNoTracking().Count();
            
            return new EarningsViewModel
            {
                MonthlyEarnings = monthlyEarnings,
                AnnualEarnings = annualEarnings,
                TotalOrders = totalOrders,
                TotalUsers = totalUsers,
                TotalProducts = totalProducts
            };
        }
    }

    // Helper class for variant updates
    public class ProductVariantUpdate
    {
        public string Size { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }

    // Helper class for variant update requests
    public class UpdateProductVariantsRequest
    {
        public int ProductId { get; set; }
        public List<ProductVariantUpdate> Variants { get; set; } = new List<ProductVariantUpdate>();
    }

    }