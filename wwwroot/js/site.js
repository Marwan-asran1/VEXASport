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

    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({
            productId: parseInt(productId),
            quantity: parseInt(quantity)
        })
    });
}

document.addEventListener('DOMContentLoaded', function () {
    if (document.querySelector('.cartpage')) {
        updateTotals();
        initializeCartListeners();
    }
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