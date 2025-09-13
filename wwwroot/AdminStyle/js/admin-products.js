/* ========================================
   VEXA Admin Panel - Products Management JavaScript
   ======================================== */

// Global variables
const productModal = document.getElementById("productModal");
const productForm = document.getElementById("productForm");
const modalTitle = document.getElementById("modalTitle");
const variantsModal = document.getElementById('variantsModal');
const variantsTitle = document.getElementById('variantsTitle');
const variantsList = document.getElementById('variantsList');
const variantForm = document.getElementById('variantForm');

// ========================================
// PRODUCT MODAL FUNCTIONS
// ========================================

function openAddProductModal() {
    modalTitle.textContent = "Add New Product";
    productForm.reset();
    document.getElementById("productId").value = "0";
    productForm.action = "/Admin/CreateProduct";
    productModal.style.display = "block";
}

function openEditModal(productId) {
    fetch(`/Admin/GetProduct/${productId}`)
        .then(res => {
            if (!res.ok) {
                throw new Error('Network response was not ok');
            }
            return res.json();
        })
        .then(product => {
            modalTitle.textContent = "Edit Product";
            document.getElementById("productId").value = product.id;
            if (product.rowVersion) {
                document.getElementById("productRowVersion").value = product.rowVersion;
            }
            document.getElementById("productName").value = product.name || "";
            document.getElementById("productDescription").value = product.description || "";
            document.getElementById("productPrice").value = product.price || "";
            document.getElementById("productImageUrl").value = product.imageUrl || "";
            document.getElementById("productGender").value = product.gender || "Men";
            document.getElementById("productType").value = product.productType || "Tops";
            
            // Clear existing variants
            const variantsContainer = document.getElementById('variantsContainer');
            variantsContainer.innerHTML = '';
            
            // Add variants from the product
            if (product.variants && product.variants.length > 0) {
                product.variants.forEach(variant => {
                    addVariantRow(variant.size, variant.stockQuantity);
                });
            } else {
                // Add default variants if none exist
                addVariantRow('S', 10);
                addVariantRow('M', 10);
                addVariantRow('L', 10);
            }
            
            productForm.action = "/Admin/UpdateProduct";
            productModal.style.display = "block";
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Error loading product data. Please try again.');
        });
}

function closeModal() {
    productModal.style.display = "none";
}

// ========================================
// VARIANTS MODAL FUNCTIONS
// ========================================

function openVariantsModal(productId, productName) {
    variantsTitle.textContent = `Variants for ${productName}`;
    variantsList.innerHTML = 'Loading...';
    document.getElementById('variantProductId').value = productId;
    fetch(`/Admin/GetProductVariants?productId=${productId}`)
      .then(r => r.json())
      .then(items => {
        if (!Array.isArray(items)) { 
            variantsList.innerHTML = 'No variants'; 
            return; 
        }
        const rows = items.map(v => `
          <div style="display:flex;justify-content:space-between;align-items:center;margin:6px 0;">
            <div><strong>Size:</strong> ${v.size} &nbsp; <strong>Stock:</strong> ${v.stockQuantity}</div>
            <form method="post" action="/Admin/DeleteVariant" onsubmit="return confirm('Delete this variant?')">
              <input type="hidden" name="id" value="${v.id}" />
              ${document.querySelector('#variantForm input[name=__RequestVerificationToken]').outerHTML}
              <button type="submit" class="btn-delete">Delete</button>
            </form>
          </div>
        `).join('');
        variantsList.innerHTML = rows || 'No variants yet';
      })
      .catch(() => variantsList.innerHTML = 'Failed to load');
    variantsModal.style.display = 'block';
}

// ========================================
// FORM VALIDATION
// ========================================

function validateForm() {
    const name = document.getElementById('productName').value.trim();
    const description = document.getElementById('productDescription').value.trim();
    const price = document.getElementById('productPrice').value;
    const gender = document.getElementById('productGender').value;
    const type = document.getElementById('productType').value;
    
    // Check variants
    const sizeSelects = document.querySelectorAll('.variant-size');
    const stockInputs = document.querySelectorAll('.variant-stock');
    let hasValidVariants = false;
    
    for (let i = 0; i < sizeSelects.length; i++) {
        const stock = parseInt(stockInputs[i].value) || 0;
        if (stock > 0) {
            hasValidVariants = true;
            break;
        }
    }

    if (!name) { alert('Please enter a product name'); return false; }
    if (!description) { alert('Please enter a product description'); return false; }
    if (!price || price <= 0) { alert('Please enter a valid price'); return false; }
    if (!gender) { alert('Please select a gender'); return false; }
    if (!type) { alert('Please select a product type'); return false; }
    if (!hasValidVariants) { alert('Please add at least one size variant with stock quantity > 0'); return false; }
    
    return true;
}

// ========================================
// VARIANT MANAGEMENT FUNCTIONS
// ========================================

function addVariant() {
    addVariantRow('S', 10);
}

function addVariantRow(size = 'S', stock = 10) {
    const container = document.getElementById('variantsContainer');
    const variantRow = document.createElement('div');
    variantRow.className = 'variant-row';
    
    variantRow.innerHTML = `
        <select class="form-control variant-size" name="VariantSizes">
            <option value="S" ${size === 'S' ? 'selected' : ''}>Small (S)</option>
            <option value="M" ${size === 'M' ? 'selected' : ''}>Medium (M)</option>
            <option value="L" ${size === 'L' ? 'selected' : ''}>Large (L)</option>
        </select>
        <input type="number" class="form-control variant-stock" name="VariantStocks" placeholder="Stock Quantity" min="0" value="${stock}">
        <button type="button" class="btn btn-danger btn-sm remove-variant" onclick="removeVariant(this)">Remove</button>
    `;
    
    container.appendChild(variantRow);
    updateRemoveButtons();
}

function removeVariant(button) {
    const variantRow = button.closest('.variant-row');
    variantRow.remove();
    updateRemoveButtons();
}

function updateRemoveButtons() {
    const container = document.getElementById('variantsContainer');
    const rows = container.querySelectorAll('.variant-row');
    
    rows.forEach((row, index) => {
        const removeBtn = row.querySelector('.remove-variant');
        if (rows.length === 1) {
            // If there's only one variant, hide the remove button
            removeBtn.style.display = 'none';
        } else {
            // If there are multiple variants, show the remove button for all
            removeBtn.style.display = 'inline-block';
        }
    });
}

// ========================================
// FORM SUBMISSION HANDLING
// ========================================

function updateProductWithVariants() {
    const productId = document.getElementById('productId').value;
    const variants = [];
    
    const sizeSelects = document.querySelectorAll('.variant-size');
    const stockInputs = document.querySelectorAll('.variant-stock');
    
    for (let i = 0; i < sizeSelects.length; i++) {
        const size = sizeSelects[i].value;
        const stock = parseInt(stockInputs[i].value) || 0;
        
        if (stock >= 0) {
            variants.push({
                Size: size,
                StockQuantity: stock
            });
        }
    }
    
    // First update the product
    const formData = new FormData(productForm);
    
    // Debug: Log what we're sending
    console.log('Sending product update with ID:', productId);
    for (let [key, value] of formData.entries()) {
        console.log(key, value);
    }
    
    fetch('/Admin/UpdateProduct', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        if (response.ok) {
            // Then update variants
            return fetch('/Admin/UpdateProductVariants', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({
                    productId: parseInt(productId),
                    variants: variants
                })
            });
        } else {
            throw new Error('Failed to update product');
        }
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert('Product updated successfully!');
            location.reload();
        } else {
            alert('Error: ' + data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Error updating product. Please try again.');
    });
}

// ========================================
// EVENT LISTENERS
// ========================================

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Wait a bit to ensure all elements are loaded
    setTimeout(() => {
        // Modal close events - use specific selectors for each modal
        const productCloseBtn = document.querySelector("#productModal .close");
        const variantsCloseBtn = document.querySelector('#variantsModal .close-variants');
        
        if (productCloseBtn) {
            productCloseBtn.onclick = function(e) {
                e.preventDefault();
                e.stopPropagation();
                closeModal();
            };
        }
        
        if (variantsCloseBtn) {
            variantsCloseBtn.onclick = function(e) {
                e.preventDefault();
                e.stopPropagation();
                variantsModal.style.display = 'none';
            };
        }
        
        // Click outside modal to close
        if (productModal) {
            productModal.onclick = function(event) {
                if (event.target === productModal) {
                    closeModal();
                }
            };
        }
        
        if (variantsModal) {
            variantsModal.onclick = function(event) {
                if (event.target === variantsModal) {
                    variantsModal.style.display = 'none';
                }
            };
        }
    }, 100);

    // Form submission handling
    if (productForm) {
        productForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            if (!validateForm()) {
                return;
            }
            
            const productId = document.getElementById('productId').value;
            const isEdit = productId !== '0';
            
            if (isEdit) {
                // For editing, we need to update variants separately
                updateProductWithVariants();
            } else {
                // For creating, use the normal form submission
                productForm.submit();
            }
        });
    }
});
