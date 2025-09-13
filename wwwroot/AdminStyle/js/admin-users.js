/* ========================================
   VEXA Admin Panel - Users Management JavaScript
   ======================================== */

// Global variables
const userModal = document.getElementById("userModal");
const userForm = document.getElementById("userForm");
const userModalTitle = document.getElementById("modalTitle");

// ========================================
// USER MODAL FUNCTIONS
// ========================================

function openAddUserModal() {
    userModalTitle.textContent = "Add New User";
    userForm.reset();
    document.getElementById("userId").value = "0";
    userForm.action = "/Admin/CreateUser";
    userForm.method = "post";
    userModal.style.display = "block";
}

function openEditUserModal(userId) {
    fetch(`/Admin/GetUser/${userId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(user => {
            userModalTitle.textContent = "Edit User";
            document.getElementById("userId").value = user.id;
            if (user.rowVersion) {
                let rv = document.getElementById('userRowVersion');
                if (!rv) {
                    rv = document.createElement('input');
                    rv.type = 'hidden';
                    rv.name = 'RowVersion';
                    rv.id = 'userRowVersion';
                    userForm.appendChild(rv);
                }
                rv.value = user.rowVersion;
            }
            document.getElementById("userName").value = user.name || "";
            document.getElementById("userEmail").value = user.email || "";
            document.getElementById("userPhone").value = user.phoneNumber || "";
            document.getElementById("userAddress").value = user.address || "";
            document.getElementById("userRole").value = user.userRole?.toString() || "";
            document.getElementById("userGender").value = user.gender?.toString() || "";
            userForm.action = "/Admin/UpdateUser";
            userForm.method = "post";
            userModal.style.display = "block";
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Error loading user data. Please try again.');
        });
}

function closeUserModal() {
    userModal.style.display = "none";
}

// ========================================
// FORM VALIDATION
// ========================================

function validateUserForm() {
    const name = document.getElementById('userName').value.trim();
    const email = document.getElementById('userEmail').value.trim();
    const phone = document.getElementById('userPhone').value.trim();
    const address = document.getElementById('userAddress').value.trim();
    const role = document.getElementById('userRole').value;
    const gender = document.getElementById('userGender').value;

    if (!name) {
        alert('Please enter a full name');
        return false;
    }

    if (!email) {
        alert('Please enter an email address');
        return false;
    }

    if (!phone) {
        alert('Please enter a phone number');
        return false;
    }

    if (phone.length !== 11) {
        alert('Phone number must be exactly 11 digits');
        return false;
    }

    if (!address) {
        alert('Please enter an address');
        return false;
    }

    if (!role) {
        alert('Please select a role');
        return false;
    }

    if (!gender) {
        alert('Please select a gender');
        return false;
    }

    return true;
}

// ========================================
// FORM SUBMISSION HANDLING
// ========================================

function handleUserFormSubmission(e) {
    // Debug: Log form data
    const formData = new FormData(userForm);
    console.log('User form data being sent:');
    for (let [key, value] of formData.entries()) {
        console.log(key + ': ' + value);
    }

    // Validate form before submission
    if (!validateUserForm()) {
        e.preventDefault();
        return false;
    }

    // Check if this is an update or create
    const userId = document.getElementById('userId').value;
    const isUpdate = userId !== '0';
    
    if (isUpdate) {
        // For updates, we need to send data as UserUpdateModel
        // Remove password field if it exists
        if (formData.has('Password')) {
            formData.delete('Password');
        }
        
        // Change the form action to UpdateUser
        userForm.action = '/Admin/UpdateUser';
    }

    const submitButton = document.getElementById('userSubmitButton');
    submitButton.disabled = true;
    submitButton.textContent = 'Saving...';
}

// ========================================
// ALERT MANAGEMENT
// ========================================

function autoHideAlerts() {
    setTimeout(function() {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(alert => {
            alert.style.display = 'none';
        });
    }, 5000);
}

// ========================================
// EVENT LISTENERS
// ========================================

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    // Wait a bit to ensure all elements are loaded
    setTimeout(() => {
        // Modal close events - use specific selector for user modal
        const userCloseBtn = document.querySelector("#userModal .close");
        
        if (userCloseBtn) {
            userCloseBtn.onclick = function(e) {
                e.preventDefault();
                e.stopPropagation();
                closeUserModal();
            };
        }
        
        // Click outside modal to close
        if (userModal) {
            userModal.onclick = function(event) {
                if (event.target === userModal) {
                    closeUserModal();
                }
            };
        }
    }, 100);

    // Form submission handling
    if (userForm) {
        userForm.addEventListener('submit', handleUserFormSubmission);
    }

    // Auto-hide alerts
    autoHideAlerts();
});
