(function () {
    // ... Keep your existing Mobile Menu code here ...
    const mobileMenuButton = document.getElementById('mobile-menu-button');
    const mobileMenu = document.getElementById('mobile-menu');

    if (mobileMenuButton && mobileMenu) {
        mobileMenuButton.addEventListener('click', () => {
            const isHidden = mobileMenu.classList.contains('hidden');
            if (isHidden) {
                mobileMenu.classList.remove('hidden');
                mobileMenu.classList.add('block');
            } else {
                mobileMenu.classList.add('hidden');
                mobileMenu.classList.remove('block');
            }
        });

        document.addEventListener('click', (event) => {
            if (!mobileMenu.contains(event.target) && !mobileMenuButton.contains(event.target)) {
                mobileMenu.classList.add('hidden');
                mobileMenu.classList.remove('block');
            }
        });
    }
})();

async function handleSubscribe(e) {
    e.preventDefault();

    const emailInput = document.getElementById('subscribeEmail');
    const submitBtn = document.getElementById('subscribeBtn');
    const errorText = document.getElementById('subscribeError');
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');

    // Reset UI
    if (errorText) errorText.classList.add('hidden');
    if (emailInput) emailInput.disabled = true;
    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerText = "Hopping...";
    }

    // Basic Client Validation
    if (!emailInput || !/^\S+@\S+\.\S+$/.test(emailInput.value)) {
        if (errorText) {
            errorText.innerText = "Please enter a valid email address.";
            errorText.classList.remove('hidden');
        }
        resetFormState(emailInput, submitBtn);
        return;
    }

    try {
        const token = tokenInput ? tokenInput.value : '';

        // Create FormData and append fields
        const formData = new FormData();
        formData.append('__RequestVerificationToken', token);
        formData.append('email', emailInput.value);

        const response = await fetch('/subscribe', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded', // Change content type
            },
            body: new URLSearchParams(formData).toString() // Convert to URL encoded
        });

        if (response.ok) {
            emailInput.value = '';
            showSubscribePopup();
        } else {
            const result = await response.json();
            throw new Error(result.message || "Something went wrong.");
        }

    } catch (error) {
        console.error('Subscription error:', error);
        if (errorText) {
            errorText.innerText = "Failed to subscribe. Try again later.";
            errorText.classList.remove('hidden');
        }
    } finally {
        resetFormState(emailInput, submitBtn);
    }
}

function resetFormState(emailInput, submitBtn) {
    if (emailInput) emailInput.disabled = false;
    if (submitBtn) {
        submitBtn.disabled = false;
        submitBtn.innerText = "Subscribe";
    }
}

function showSubscribePopup() {
    const popup = document.getElementById('subscribePopup');
    if (popup) {
        popup.classList.remove('hidden');
        popup.setAttribute('aria-hidden', 'false');
    }
}

function closeSubscribePopup() {
    const popup = document.getElementById('subscribePopup');
    if (popup) {
        popup.classList.add('hidden');
        popup.setAttribute('aria-hidden', 'true');
    }
}

// Close popup when clicking outside
document.addEventListener('click', (e) => {
    const popup = document.getElementById('subscribePopup');
    if (popup && !popup.classList.contains('hidden') && !popup.contains(e.target) && !e.target.closest('button[onclick="showSubscribePopup()"]')) {
        closeSubscribePopup();
    }
});