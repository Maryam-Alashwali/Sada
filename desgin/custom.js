// User type selection
document.querySelectorAll('.user-type-option').forEach(option => {
    option.addEventListener('click', function() {
        document.querySelectorAll('.user-type-option').forEach(opt => {
            opt.classList.remove('selected');
        });
        this.classList.add('selected');

        // Show/hide tailor-specific fields
        const userType = this.getAttribute('data-type');
        const tailorFields = document.getElementById('tailorFields');

        if (userType === 'tailor') {
            tailorFields.style.display = 'block';
        } else {
            tailorFields.style.display = 'none';
        }
    });
});

// Password strength indicator
document.getElementById('password').addEventListener('input', function() {
    const password = this.value;
    const strengthBar = document.getElementById('passwordStrength');

    // Reset classes
    strengthBar.className = 'password-strength-bar';

    if (password.length === 0) {
        strengthBar.style.width = '0%';
        return;
    }

    // Calculate strength
    let strength = 0;

    // Length check
    if (password.length >= 8) strength += 25;

    // Contains lowercase
    if (/[a-z]/.test(password)) strength += 25;

    // Contains uppercase
    if (/[A-Z]/.test(password)) strength += 25;

    // Contains numbers
    if (/[0-9]/.test(password)) strength += 25;

    // Update strength bar
    strengthBar.style.width = strength + '%';

    // Add appropriate class
    if (strength <= 25) {
        strengthBar.classList.add('password-weak');
    } else if (strength <= 50) {
        strengthBar.classList.add('password-medium');
    } else if (strength <= 75) {
        strengthBar.classList.add('password-strong');
    } else {
        strengthBar.classList.add('password-very-strong');
    }
});

// Form validation
document.getElementById('registerForm').addEventListener('submit', function(e) {
    e.preventDefault();

    const firstName = document.getElementById('firstName').value;
    const lastName = document.getElementById('lastName').value;
    const email = document.getElementById('email').value;
    const phone = document.getElementById('phone').value;
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    const terms = document.getElementById('terms').checked;
    const userType = document.querySelector('.user-type-option.selected').getAttribute('data-type');

    // Simple validation
    if (!firstName || !lastName || !email || !phone || !password || !confirmPassword) {
        alert('Please fill in all required fields');
        return;
    }

    if (password !== confirmPassword) {
        alert('Passwords do not match');
        return;
    }

    if (password.length < 8) {
        alert('Password must be at least 8 characters long');
        return;
    }

    if (!terms) {
        alert('Please agree to the Terms of Service and Privacy Policy');
        return;
    }

    // Here you would typically send the data to your server
    // For demo purposes, we'll just show a success message
    alert(`Registration successful! Welcome to Sada, ${firstName}. Redirecting to your dashboard...`);
    // In a real application, you would redirect to the appropriate dashboard
    // if (userType === 'tailor') {
    //     window.location.href = 'tailor-dashboard.html';
    // } else {
    //     window.location.href = 'client-dashboard.html';
    // }
});