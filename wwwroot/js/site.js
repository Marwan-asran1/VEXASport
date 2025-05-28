function OpenFromImg(productId) {
    window.location.href = '/Products/Details/' + productId;
}

function updateTotals() {
    let grandTotal = 0;
    let items = document.querySelectorAll('.cartitem');
    items.forEach(function (item) {
        let input = item.querySelector('.qty');
        let quantity = parseFloat(input.value);
        let price = parseFloat(input.dataset.price);
        let total = quantity * price;

        let totalprice = item.querySelector('.totalprice');
        totalprice.textContent = 'EGP ' + total.toFixed(2);
        grandTotal += total;
    });
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
            
        })
    });

    //document.querySelectorAll('.qty').forEach(function (input) {
    //    input.addEventListener('change', function () {
    //        updateTotals();
    //        const productId = this.closest('.cartitem').querySelector('input[name="productId"]').value;
    //        sendQuantityUpdate(productId);
    //    });
    //});
}

function sendQuantityUpdate(productId) {
    const input = document.querySelector(`.cartitem input[name="productId"][value="${productId}"]`).closest('.cartitem').querySelector('.qty');
    const quantity = input.value;

    if (parseInt(quantity) < 1) {
        input.value = 1;
        updateTotals();
        return;
    }
    fetch(`/Cart/UpdateQuantity?productId=${productId}&quantity=${quantity}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    });
}

document.addEventListener('DOMContentLoaded', function () {
    if (document.querySelector('.cartpage')) {
        updateTotals();
        initializeCartListeners();
    }
});

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

const buttons = document.querySelectorAll('.addCart');
const B = document.querySelectorAll('.add-to-cart-btn ');
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
B.forEach(B => {
    B.addEventListener('click', () => {
        B.innerHTML = '✔ Added';
        B.style.backgroundColor = 'green';
        B.disabled = true;

        setTimeout(() => {
            B.innerHTML = 'Add To Cart';
            B.style.backgroundColor = '#333';
            B.disabled = false;
        }, 2000);
    });
});
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