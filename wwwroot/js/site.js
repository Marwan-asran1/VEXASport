function OpenFromImg(productId) {
    
    window.location.href = '/Products/Details/' + productId;
}

function updateTotals() {
    let grandTotal = 0;

    // Calculate totals for each item
    document.querySelectorAll('.cartitem').forEach(function (item) {
        const input = item.querySelector('.qty');
        const price = parseFloat(input.dataset.price);
        const quantity = parseInt(input.value);
        const itemTotal = price * quantity;

        // Update the item's total price display
        item.querySelector('.totalprice').textContent = 'EGP ' + itemTotal.toFixed(2);

        // Add to grand total
        grandTotal += itemTotal;
    });

    // Update all instances of subtotal and grandtotal
    document.querySelectorAll('.subtotal').forEach(function (el) {
        el.textContent = 'EGP ' + grandTotal.toFixed(2);
    });

    // Fixed selector to use class instead of tag name
    document.querySelectorAll('.grandtotal').forEach(function (el) {
        el.textContent = 'EGP ' + grandTotal.toFixed(2);
    });
}

// Attach event listeners to quantity inputs
function initializeCartListeners() {
    document.querySelectorAll('.qty').forEach(function (input) {
        input.addEventListener('change', function () {
            updateTotals();
            sendQuantityUpdate(this);
        });
    });
}

function sendQuantityUpdate(input) {
    const productId = input.closest('.cartitem').querySelector('input[name="productId"]').value;
    const size = input.closest('.cartitem').querySelector('input[name="size"]').value;
    const quantity = input.value;

    // Don't allow quantity less than 1
    if (parseInt(quantity) < 1) {
        input.value = 1;
        updateTotals();
        return;
    }

    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({
            productId: parseInt(productId),
            size: size,
            quantity: parseInt(quantity)
        })
    });
}

// Initialize cart functionality on page load
document.addEventListener('DOMContentLoaded', function () {
    if (document.querySelector('.cartpage')) {
        updateTotals();
        initializeCartListeners();
    }
});
// Function to handle size selection
function initializeSizeSelection() {
    const sizeBoxes = document.querySelectorAll('.size');
    const selectedSizeInput = document.getElementById('selectedSize');

    sizeBoxes.forEach(box => {
        box.addEventListener('click', function () {
            // Remove selected class from all boxes
            sizeBoxes.forEach(b => b.classList.remove('selected'));

            // Add selected class to clicked box
            this.classList.add('selected');

            // Update the hidden input value
            selectedSizeInput.value = this.dataset.size;
        });
    });
}

// Initialize size selection when the page loads
document.addEventListener('DOMContentLoaded', function () {
    if (document.querySelector('.T1-sizes')) {
        initializeSizeSelection();
    }
});
document.querySelector('form').addEventListener('submit', function (e) {
    const selectedSize = document.getElementById('selectedSize').value;
    if (!selectedSize) {
        e.preventDefault(); // Prevent form submission
        alert('Please select a size before adding to cart');
    }
});