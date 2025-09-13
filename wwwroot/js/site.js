function OpenFromImg(productId) {
    window.location.href = '/Products/Details/' + productId;
}

function updateTotals() {
    let grandTotal = 0;
    
    // Check for desktop cart items
    let items = document.querySelectorAll('.cartitem');
    if (items.length > 0) {
        // Desktop cart logic
        items.forEach(function (item) {
            let input = item.querySelector('.qty');
            let quantity = parseFloat(input.value);
            let price = parseFloat(input.dataset.price);
            let total = quantity * price;

            let totalprice = item.querySelector('.totalprice');
            totalprice.textContent = 'EGP ' + total.toFixed(2);
            grandTotal += total;
        });
    } else {
        // Mobile cart logic - don't override the server-calculated total
        // The total is already calculated in the server and displayed correctly
        return; // Exit early to avoid overriding the correct total
    }
    
    document.querySelectorAll('.subtotal').forEach(function (sub) {
        sub.textContent = 'EGP ' + grandTotal.toFixed(2);
    });

    document.querySelectorAll('.grandtotal').forEach(function (gt) {
        gt.textContent = 'EGP ' + grandTotal.toFixed(2);
    });
}

function initializeCartListeners() {
    let inputs = document.querySelectorAll('.qty');
    inputs.forEach(function (input) {
        input.addEventListener('change', function () {
            updateTotals();
            const productId = this.closest('.cartitem').querySelector('input[name="productId"]').value;
            const size = this.closest('.cartitem').querySelector('input[name="size"]').value;
            sendQuantityUpdate(productId, size);
        })
    });
}

function sendQuantityUpdate(productId, size) {
    const input = document.querySelector(`.cartitem input[name="productId"][value="${productId}"]`).closest('.cartitem').querySelector('.qty');
    const quantity = input.value;

    if (parseInt(quantity) < 1) {
        input.value = 1;
        updateTotals();
        return;
    }

    fetch(`/Cart/UpdateQuantity?productId=${productId}&quantity=${quantity}&size=${size}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
    .then(response => response.json())
    .then(data => {
        if (!data.success) {
            alert(data.message || 'Failed to update quantity');
            // Reset to previous value or 1
            input.value = 1;
            updateTotals();
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Failed to update quantity');
        input.value = 1;
        updateTotals();
    });
}

function clearCart() {
    if (confirm('Are you sure you want to clear your cart?')) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (!token) {
            alert('Security token not found. Please refresh the page and try again.');
            return;
        }
        
        fetch('/Cart/ClearCart', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token.value
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateCartCounter(0);
                location.reload();
            } else {
                alert('Failed to clear cart');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Failed to clear cart');
        });
    }
}

document.addEventListener('DOMContentLoaded', function () {
    if (document.querySelector('.cartpage')) {
        // Only run updateTotals for desktop cart (with .cartitem elements)
        if (document.querySelector('.cartitem')) {
            updateTotals();
            initializeCartListeners();
        } else {
            // For mobile cart, protect the server-calculated total
            protectMobileCartTotal();
        }
    }
    
    // Initialize add to cart buttons
    initializeAddToCartButtons();
});

function protectMobileCartTotal() {
    // Store the original total value
    const totalElement = document.getElementById('cart-total-display');
    if (totalElement) {
        const originalTotal = totalElement.textContent;
        
        // Monitor for any changes and restore if needed
        const observer = new MutationObserver(function(mutations) {
            mutations.forEach(function(mutation) {
                if (mutation.type === 'childList' || mutation.type === 'characterData') {
                    if (totalElement.textContent !== originalTotal && !totalElement.textContent.includes('EGP')) {
                        totalElement.textContent = originalTotal;
                    }
                }
            });
        });
        
        observer.observe(totalElement, {
            childList: true,
            characterData: true,
            subtree: true
        });
    }
}

function updateCartCounter(count) {
    let cartCounter = document.querySelector(".CartCounter"); 
    if (count > 0) {
        cartCounter.textContent = count;
        cartCounter.style.display = 'block';
    } else {
        cartCounter.style.display = 'none';
    }
}

function addToCart(productId) {
    // Check if the product is sold out
    const soldOutBtn = document.querySelector('.sold-out-btn');
    if (soldOutBtn) {
        alert('This product is sold out and cannot be added to cart');
        return;
    }

    const sizeInput = document.getElementById('selectedSize');
    if (!sizeInput || !sizeInput.value) {
        alert('Please select a size before adding to cart');
        return;
    }

    const size = sizeInput.value;
    const URL = `/Cart/AddToCart?id=${productId}&size=${size}`;
    
    fetch(URL)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateCartCounter(data.cartcounter);
                const button = document.querySelector('.add-to-cart-btn');
                if (button) {
                    button.innerHTML = '✔ Added';
                    button.style.backgroundColor = 'green';
                    button.disabled = true;

                    setTimeout(() => {
                        button.innerHTML = 'Add To Cart';
                        button.style.backgroundColor = '#00105c';
                        button.disabled = false;
                    }, 2000);
                }
            } else {
                alert(data.message || 'Failed to add to cart');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Failed to add to cart');
        });
}

function initializeAddToCartButtons() {
    const buttons = document.querySelectorAll('.addCart');
    const addToCartButtons = document.querySelectorAll('.add-to-cart-btn');
    
    buttons.forEach(button => {
        button.addEventListener('click', () => {
            button.innerHTML = '✔ Added';
            button.style.backgroundColor = 'green';
            button.disabled = true;

            setTimeout(() => {
                button.innerHTML = '<i class="fa-solid fa-basket-shopping"></i>';
                button.style.backgroundColor = '#212529';
                button.disabled = false;
            }, 2000);
        });
    });
    
    addToCartButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            btn.innerHTML = '✔ Added';
            btn.style.backgroundColor = 'green';
            btn.disabled = true;

            setTimeout(() => {
                btn.innerHTML = 'Add To Cart';
                btn.style.backgroundColor = '#333';
                btn.disabled = false;
            }, 2000);
        });
    });
}
//function initializeSizeSelection() {
//    const sizeBoxes = document.querySelectorAll('.size');
//    const selectedSizeInput = document.getElementById('selectedSize');
//
//    sizeBoxes.forEach(box => {
//        box.addEventListener('click', function () {
//            sizeBoxes.forEach(b => b.classList.remove('selected'));
//            this.classList.add('selected');
//            selectedSizeInput.value = this.dataset.size;
//        });
//    });
//}

//document.addEventListener('DOMContentLoaded', function () {
//    if (document.querySelector('.T1-sizes')) {
//        initializeSizeSelection();
//    }
//});

//document.querySelector('form').addEventListener('submit', function (e) {
//    const selectedSize = document.getElementById('selectedSize').value;
//    if (!selectedSize) {
//        e.preventDefault();
//        alert('Please select a size before adding to cart');
//    }
//});