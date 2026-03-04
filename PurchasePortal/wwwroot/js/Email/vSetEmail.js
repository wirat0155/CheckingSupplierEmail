function editUsername(h3Element, originalUsername) {
    // ป้องกันสร้าง input ซ้ำ
    if (h3Element.style.display === 'none') return;

    // สร้าง <input> element
    const input = document.createElement('input');
    input.type = 'text';
    input.className = 'username-input form-control';
    input.value = originalUsername;

    // ซ่อน <h3> ชั่วคราว
    h3Element.style.display = 'none';
    // แทรก <input> หลัง <h3>
    h3Element.parentNode.insertBefore(input, h3Element.nextSibling);
    input.focus();

    // กด Enter → save
    input.onkeydown = function (event) {
        if (event.key === 'Enter') {
            saveUsername(input, h3Element, originalUsername);
        } else if (event.key === 'Escape') {
            // ESC → คืนค่าเดิม
            input.remove();
            h3Element.style.display = '';
        }
    };

    // onblur → คืนค่าเดิม ไม่ update
    input.onblur = function () {
        input.remove();
        h3Element.style.display = '';
    };
}

async function saveUsername(inputElement, h3Element, originalUsername) {
    const newUsername = inputElement.value.trim();

    // ถ้าไม่มีการเปลี่ยนแปลง หรือค่าว่าง → ยกเลิก
    if (newUsername === '' || newUsername === originalUsername) {
        inputElement.remove();
        h3Element.style.display = '';
        return;
    }

    try {
        const response = await $.post(`${basePath}/Email/SetUsername`, {
            txt_oldusername: originalUsername,
            txt_newusername: newUsername
        });

        const { success, text = "", errors, empnameeng = "" } = response;

        if (success) {
            SwalOK(text || "Username updated successfully!");
            h3Element.textContent = newUsername;
            h3Element.setAttribute('onclick', `editUsername(this, '${newUsername}')`);

            // อัปเดต <p> ข้างล่าง
            const pElement = h3Element.nextElementSibling;
            if (pElement && pElement.tagName === 'P') {
                pElement.textContent = empnameeng;
            }
        } else {
            SwalNG(errors || "Failed to update username.");
        }
    } catch (error) {
        console.error("Save username error:", error);
        SwalNG("Connection Error");
    } finally {
        // Cleanup UI
        if (document.body.contains(inputElement)) inputElement.remove();
        h3Element.style.display = '';
    }
}

async function deleteUser(username) {
    // แสดง confirm box
    const isConfirmed = confirm(`คุณต้องการลบผู้ใช้ ${username} ใช่หรือไม่?`);

    if (!isConfirmed) return; // ถ้าไม่กดยืนยันให้จบเลย

    try {
        // เรียก AJAX post
        const response = await $.post(`${basePath}/Email/DeleteUsername`, {
            txt_username: username
        });

        const { success, text = "", errors } = response;

        // ตรวจสอบผลลัพธ์
        if (success) {
            SwalOK(text || "Username remove successfully!");
            // หา div หลักที่ครอบทั้ง header + body แล้ว fade ออก
            const card = $(`button[onclick="deleteUser('${username}')"]`).closest('.user-card');
            card.fadeOut(500, function () {
                $(this).remove();
            });
        } else {
            SwalNG(errors || "Failed to remove username.");
        }
    } catch (error) {
        console.error("Remove username error:", error);
        SwalNG("Connection Error");
    }
}

function addEmail(username) {
        // หา section ของ user ตาม username
        let userSection = null;
    document.querySelectorAll('.user-card').forEach(card => {
        const uname = card.querySelector('.editable-username')?.textContent.trim();
    if (uname === username) {
        userSection = card;
        }
    });

    if (!userSection) {
        alert('User not found.');
    return;
    }

    // หา email section ของ user นั้น
    const emailSection = userSection.querySelector('.data-section:nth-of-type(2) .item-list');
    if (!emailSection) {
        alert('Email section not found.');
    return;
    }

    // สร้าง input ใหม่
    const inputDiv = document.createElement('div');
    inputDiv.classList.add('item-card');

    const input = document.createElement('input');
    input.type = 'email';
    input.placeholder = 'Enter new email';
    input.classList.add('form-control');

    inputDiv.appendChild(input);
    emailSection.appendChild(inputDiv);
    input.focus();

    input.addEventListener('keydown', async function (e) {
        if (e.key === 'Enter') {
            const newEmail = input.value.trim();
            if (!newEmail) return alert('Please enter an email.');

            try {
                const response = await $.post(`${basePath}/Email/AddEmail`, {
                    txt_id: 0,
                    txt_username: username,
                    txt_email: newEmail
                });

                const { success, text = "", errors } = response;

                if (success) {
                    SwalOK(text || "Add email successfully!");
                    setTimeout(() => {
                        location.reload();
                    }, 500);
                } else {
                    SwalNG(errors || "Failed to add email.");
                }
            } catch (error) {
                console.error("Add email error:", error);
                SwalNG("Connection Error");
            }
        }
    });

}

function editEmail(pElement, emailId) {
    const currentValue = pElement.textContent.trim();

    // ป้องกันสร้าง input ซ้ำ
    if (pElement.tagName === 'INPUT') return;

    // สร้าง input
    const input = document.createElement('input');
    input.type = 'email';
    input.value = currentValue;
    input.classList.add('form-control');

    // แทน p ด้วย input
    pElement.replaceWith(input);
    input.focus();

    // กด Enter → updateEmail
    input.onkeydown = function (e) {
        if (e.key === 'Enter') {
            const newValue = input.value.trim();
            if (!newValue) return alert('Email cannot be empty');

            updateEmail(emailId, newValue, input);
        }
        // กด Esc → ยกเลิก
        else if (e.key === 'Escape') {
            input.replaceWith(pElement);
        }
    };

    // เลิก focus → คืน p เดิม
    input.onblur = function () {
        input.replaceWith(pElement);
    };
}


// updateEmail function
function updateEmail(emailId, newValue, inputElement) {
    $.post(`${basePath}/Email/UpdateEmail`, { txt_id: emailId, txt_email: newValue })
        .done(response => {
            const { success, text = "", errors } = response;
            if (success) {
                SwalOK(text || "Email updated!");
                setTimeout(() => {
                    location.reload();
                }, 500);
            } else {
                SwalNG(errors || "Failed to update email.");
            }
        })
        .fail(() => {
            SwalNG("Connection Error");
        });
}

async function deleteItem(type, id) {
    if (!id || !type) return;

    const confirmDelete = await confirm(`Are you sure you want to delete this ${type}?`);
    if (!confirmDelete) return;

    try {
        // map endpoint และชื่อ field ตามประเภท
        const config = {
            email: { url: `${basePath}/Email/DeleteById`, field: "txt_id", successMsg: "Email deleted successfully!" },
            empno: { url: `${basePath}/Email/DeleteById`, field: "txt_id", successMsg: "Employee deleted successfully!" }
        };

        const { url, field, successMsg } = config[type.toLowerCase()] || {};
        if (!url) return SwalNG("Invalid delete type.");

        // เรียก AJAX
        const response = await $.post(url, { [field]: id });
        const { success, text = "", errors } = response;

        if (success) {
            SwalOK(text || successMsg);
            setTimeout(() => location.reload(), 500);
        } else {
            SwalNG(errors || `Failed to delete ${type}.`);
        }

    } catch (error) {
        console.error(`Delete ${type} error:`, error);
        SwalNG("Connection Error");
    }
}

function editEmpno(spanElement, empId) {
    const currentValue = spanElement.textContent.trim();

    // ป้องกันสร้าง input ซ้ำ
    if (spanElement.tagName === 'INPUT') return;

    // สร้าง input
    const input = document.createElement('input');
    input.type = 'text';
    input.value = currentValue;
    input.classList.add('form-control');

    // แทน span ด้วย input
    spanElement.replaceWith(input);
    input.focus();

    // กด Enter → updateEmpno
    input.onkeydown = function (e) {
        if (e.key === 'Enter') {
            const newValue = input.value.trim();
            if (!newValue) return alert('Employee No. cannot be empty');

            updateEmpno(empId, newValue, input);
        }
        // กด Esc → ยกเลิก
        else if (e.key === 'Escape') {
            input.replaceWith(spanElement);
        }
    };

    // เลิก focus → คืน span เดิม
    input.onblur = function () {
        input.replaceWith(spanElement);
    };
}

async function updateEmpno(empId, newValue, inputEl) {
    try {
        const response = await $.post(`${basePath}/Email/UpdateEmpno`, { txt_id: empId, txt_empno: newValue });
        const { success, text = "", errors } = response;

        if (success) {
            SwalOK(text || "Employee number updated!");
            setTimeout(() => {
                location.reload();
            }, 500);
        } else {
            SwalNG(errors || "Failed to update employee number.");
            inputEl.replaceWith(document.createTextNode(newValue));
        }
    } catch (error) {
        console.error("Update employee error:", error);
        SwalNG("Connection Error");
    }
}

async function addEmpno(username) {
    // หา section ของ user ตาม username
    let userSection = null;
    document.querySelectorAll('.user-card').forEach(card => {
        const uname = card.querySelector('.editable-username')?.textContent.trim();
        if (uname === username) {
            userSection = card;
        }
    });

    if (!userSection) {
        alert('User not found.');
        return;
    }

    // หา empno section ของ user นั้น
    const empSection = userSection.querySelector('.data-section:nth-of-type(1) .item-list');
    if (!empSection) {
        alert('Employee section not found.');
        return;
    }

    // สร้าง input ใหม่
    const inputDiv = document.createElement('div');
    inputDiv.classList.add('item-card');

    const input = document.createElement('input');
    input.type = 'text';
    input.placeholder = 'Enter new employee number';
    input.classList.add('form-control');

    inputDiv.appendChild(input);
    empSection.appendChild(inputDiv);
    input.focus();

    input.addEventListener('keydown', async function (e) {
        if (e.key === 'Enter') {
            const newEmpno = input.value.trim();
            if (!newEmpno) return alert('Please enter an employee number.');

            try {
                // เรียก AJAX ไปยัง endpoint เพิ่ม Empno
                const response = await $.post(`${basePath}/Email/AddEmpno`, {
                    txt_id: 0,
                    txt_username: username,
                    txt_empno: newEmpno
                });

                const { success, text = "", errors } = response;

                if (success) {
                    SwalOK(text || "Employee number added successfully!");
                    setTimeout(() => {
                        location.reload();
                    }, 500);
                } else {
                    SwalNG(errors || "Failed to add employee number.");
                }
            } catch (error) {
                console.error("Add employee error:", error);
                SwalNG("Connection Error");
            }
        }
    });
}

function addUsername() {
    const container = document.querySelector('.page-header');
    if (!container) return alert("Page header not found.");

    // ป้องกันการสร้าง input ซ้ำ
    if (document.querySelector('.new-user-card')) return;

    // สร้าง section user ใหม่
    const section = document.createElement('section');
    section.classList.add('user-card', 'new-user-card');

    section.innerHTML = `
        <header class="user-card-header">
            <div class="user-info">
                <h3 class="editable-username" title="Click to edit username">
                    <input type="text" class="form-control new-username-input" placeholder="Enter username" />
                </h3>
                <p>(new user)</p>
            </div>
            <button class="btn btn-outline-danger" type="button" onclick="this.closest('.user-card').remove()">
                Remove
            </button>
        </header>

        <div class="user-card-body">
            <div class="data-section">
                <div class="section-header">
                    <h4>Employee Numbers</h4>
                    <button class="btn btn-add-item" type="button" onclick="addEmpno(getNewUsername(this))">
                        <i class="fa fa-plus"></i> Add
                    </button>
                </div>
                <div class="item-list">
                    <p>No employee numbers assigned.</p>
                </div>
            </div>

            <div class="data-section">
                <div class="section-header">
                    <h4>Additional Emails</h4>
                    <button class="btn btn-add-item" type="button" onclick="addEmail(getNewUsername(this))">
                        <i class="fa fa-plus"></i> Add
                    </button>
                </div>
                <div class="item-list">
                    <p>No additional emails assigned.</p>
                </div>
            </div>
        </div>
    `;

    // แทรก user-card ใหม่ต่อท้าย
    container.insertAdjacentElement('afterend', section);

    // โฟกัสที่ input
    const input = section.querySelector('.new-username-input');
    input.focus();

    // เมื่อกรอกชื่อแล้วกด Enter → เปลี่ยนเป็นชื่อปกติ
    input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            const newUsername = input.value.trim();
            if (!newUsername) return alert('Please enter a username.');

            const h3 = input.closest('.editable-username');
            h3.textContent = newUsername;
        }
    });
}

// ฟังก์ชันช่วยดึง username ล่าสุดจาก user card
function getNewUsername(element) {
    const card = element.closest('.user-card');
    const usernameEl = card.querySelector('.editable-username');
    return usernameEl.textContent.trim();
}
