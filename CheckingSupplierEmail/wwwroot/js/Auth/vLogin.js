async function Login(event) {
    //showLoader();
    removeError();
    event.preventDefault();

    try {
        const form = event.target;
        const submitButton = event.submitter;

        const formData = new FormData(form);
        if (submitButton && submitButton.name) {
            formData.append(submitButton.name, submitButton.value);
        }

        const response = await fetch(`${basePath}/Auth/Login`, {
            method: 'POST',
            body: formData
        });

        const { success, text = "", errors = [] } = await response.json();
        console.log(success, text, errors);
        if (!success) {
            showError(errors);
            SwalNG(errors, text);
        }
        else {
            window.location.pathname = `${basePath}/Email/vSetEmail`;
        }
    } catch ({ message }) {
        alert(`Exception: ${message}`);
    }
    //hideLoader();
}