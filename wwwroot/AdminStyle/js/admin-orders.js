/* ========================================
   VEXA Admin Panel - Orders Management JavaScript
   ======================================== */

// Global variables
const orderDetailsModal = document.getElementById('orderDetailsModal');
const orderDetailsContent = document.getElementById('orderDetailsContent');

// ========================================
// ORDER DETAILS MODAL FUNCTIONS
// ========================================

function showOrderDetails(orderId) {
    // Check if modal elements exist
    if (!orderDetailsModal || !orderDetailsContent) {
        console.error('Order details modal elements not found');
        return;
    }
    
    // Show modal with loading
    orderDetailsContent.innerHTML = '<div class="loading">Loading order details...</div>';
    orderDetailsModal.style.display = 'block';

    // Fetch order details
    fetch(`/Admin/GetOrderDetails?id=${orderId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                displayOrderDetails(data.order);
            } else {
                orderDetailsContent.innerHTML = '<div class="loading">Error: ' + data.message + '</div>';
            }
        })
        .catch(error => {
            orderDetailsContent.innerHTML = '<div class="loading">Error loading order details</div>';
            console.error('Error:', error);
        });
}

function displayOrderDetails(order) {
    if (!orderDetailsContent) {
        console.error('Order details content element not found');
        return;
    }
    
    const itemsHtml = order.items.map(item => `
        <tr>
            <td>${item.productName}</td>
            <td>${item.productGender} - ${item.productType}</td>
            <td>${item.size}</td>
            <td>${item.quantity}</td>
            <td>$${item.unitPrice}</td>
            <td>$${item.totalPrice}</td>
        </tr>
    `).join('');

    orderDetailsContent.innerHTML = `
        <div class="order-info">
            <div class="order-info-section">
                <h3>Order Information</h3>
                <div class="info-item">
                    <span class="info-label">Order ID:</span>
                    <span class="info-value">#${order.id}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Order Date:</span>
                    <span class="info-value">${order.orderDate}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Status:</span>
                    <span class="info-value">${order.status}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Payment Method:</span>
                    <span class="info-value">${order.method}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Delivered Date:</span>
                    <span class="info-value">${order.deliveredDate}</span>
                </div>
            </div>
            <div class="order-info-section">
                <h3>Customer Information</h3>
                <div class="info-item">
                    <span class="info-label">Name:</span>
                    <span class="info-value">${order.customerName}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Email:</span>
                    <span class="info-value">${order.customerEmail}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Phone:</span>
                    <span class="info-value">${order.contactPhone}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Shipping Address:</span>
                    <span class="info-value">${order.shippingAddress}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Billing Address:</span>
                    <span class="info-value">${order.billingAddress}</span>
                </div>
            </div>
        </div>
        <div class="order-items">
            <h3>Order Items</h3>
            <table class="items-table">
                <thead>
                    <tr>
                        <th>Product</th>
                        <th>Category</th>
                        <th>Size</th>
                        <th>Quantity</th>
                        <th>Unit Price</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    ${itemsHtml}
                </tbody>
            </table>
            <div class="order-total">
                Total: $${order.orderTotal}
            </div>
        </div>
    `;
}

// ========================================
// EVENT LISTENERS
// ========================================

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Wait a bit to ensure all elements are loaded
    setTimeout(() => {
        // Close modal when clicking X - use specific selector
        const orderCloseBtn = document.querySelector('#orderDetailsModal .close');
        if (orderCloseBtn && orderDetailsModal) {
            orderCloseBtn.onclick = function(e) {
                e.preventDefault();
                e.stopPropagation();
                orderDetailsModal.style.display = 'none';
            };
        }

        // Close modal when clicking outside
        if (orderDetailsModal) {
            orderDetailsModal.onclick = function(event) {
                if (event.target === orderDetailsModal) {
                    orderDetailsModal.style.display = 'none';
                }
            };
        }
    }, 100);

    // Handle view order button clicks
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('view-order-btn')) {
            const orderId = e.target.getAttribute('data-order-id');
            showOrderDetails(orderId);
        }
    });
});
