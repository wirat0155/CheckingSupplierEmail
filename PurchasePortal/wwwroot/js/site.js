function SwalOK(text) {
    Swal.fire({
        title: "OK",
        text: text,
        icon: "success",
        timer: 2000
    });
}
function SwalNG(errors, text) {
    // แสดงเฉพาะ errorMessage จากสมาชิก [0] เท่านั้น
    if (Array.isArray(errors) && errors.length > 0) {
        const { property, errorMessage } = errors[0];
        Swal.fire({
            title: "NG",
            html: errorMessage,
            icon: "error"
        });
    } else {
        // fallback กรณีไม่ได้ส่ง array มา
        Swal.fire({
            title: "NG",
            html: `${errors ?? ""} ${text ?? ""}`,
            icon: "error"
        });
    }
}

$('form').on('input', 'input, select, textarea', function () {
    const inputName = $(this).attr('name');
    if (inputName) {
        // Escape special characters for the property name in the error object
        const escapedInputName = inputName.replace(/([.#\[\]\\'"])/g, '\\$&');
        $('.error#' + escapedInputName).text('');
    }
});

function removeError() {
    $(".error").text("");
}

function showError(errors) {
    const escapeSelector = (selector) => selector.replace(/([.#\[\]\\'"])/g, '\\$&'); // Properly closed group

    errors.forEach(error => {
        const escapedProperty = escapeSelector(error.property);
        $(".error#" + escapedProperty).text(error.errorMessage);
    });

    const topmostElement = errors
        .map(error => {
            const escapedProperty = escapeSelector(error.property);
            return $(`[name="${escapedProperty}"]`)[0];
        })
        .sort((a, b) => a.getBoundingClientRect().top - b.getBoundingClientRect().top)[0];

    if (topmostElement) {
        $(topmostElement).focus();
    }
}




function renderErrorSpans() {
    $('input[data-form], select[data-form], textarea[data-form]').each(function () {
        const formName = $(this).attr('data-form');
        if (!$('#' + formName).length) {
            $('<p>', { class: 'error text-danger', id: formName }).insertAfter(this);
        }
    });
}

const basePath = (window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1') ? "/" + window.location.pathname.split('/')[1] : ''; // Adjust '/myapp' to your subfolder name

$('form').on('keydown', 'input, select, textarea', function (e) {
    if (e.key === 'Enter') {
        e.preventDefault();
    }
});

    function addEmail(username) {
    const container = document.querySelector('.item-list');

    // สร้าง input ใหม่
    const inputDiv = document.createElement('div');
    inputDiv.classList.add('item-card');

    const input = document.createElement('input');
    input.type = 'email';
    input.placeholder = 'Enter new email';
    input.classList.add('form-control');
    input.autofocus = true;

    inputDiv.appendChild(input);
    container.appendChild(inputDiv);

    // ฟัง enter เพื่อ save
    input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            const newEmail = input.value.trim();
    if (!newEmail) return alert('Please enter an email.');

    // ยิง AJAX ไปเซฟ
    fetch('/User/AddEmail', {
        method: 'POST',
    headers: {
        'Content-Type': 'application/json'
                },
    body: JSON.stringify({username: username, email: newEmail })
            })
            .then(res => {
                if (!res.ok) throw new Error('Failed to save email');
    return res.json();
            })
            .then(data => {
        // แทน input ด้วย display ปกติ
        inputDiv.innerHTML = `
                    <div class="item-card-details">
                        <p>${newEmail}</p>
                    </div>
                    <button class="btn btn-outline-danger" type="button"
                        onclick="deleteEmail('${data.id}')" title="Delete Email">
                        Remove
                    </button>
                `;
            })
            .catch(err => {
        alert(err.message);
    input.focus();
            });
        }
    });

    // focus ทันที
    input.focus();
}
