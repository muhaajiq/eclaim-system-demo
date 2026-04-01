window.jsInterop = window.jsInterop || {};

window.jsInterop.showModal = function (selector) {
    var modal = new bootstrap.Modal(document.querySelector(selector));
    modal.show();
};

window.jsInterop.hideModal = function (selector) {
    var modal = bootstrap.Modal.getInstance(document.querySelector(selector));
    if (modal) modal.hide();
};

window.jsInterop.registerOnHidden = function (selector, dotNetRef) {
    var el = document.querySelector(selector);
    if (!el) return;
    if (el.__blazorHiddenRegistered) return;

    const handler = () => {
        dotNetRef.invokeMethodAsync('OnModalHidden');
    };

    el.addEventListener('hidden.bs.modal', handler);
    el.__blazorHiddenHandler = handler;
    el.__blazorHiddenRegistered = true;
};

window.jsInterop.unregisterOnHidden = function (selector) {
    var el = document.querySelector(selector);
    if (!el || !el.__blazorHiddenHandler) return;
    el.removeEventListener('hidden.bs.modal', el.__blazorHiddenHandler);
    delete el.__blazorHiddenHandler;
    delete el.__blazorHiddenRegistered;
};

window.jsInterop.scrollToTop = function () {
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
};

window.jsInterop.scrollToError = function (id) {
    var el = document.getElementById(id);
    if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};

window.jsInterop.createBlobUrl = function (base64, mimeType) {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: mimeType });

    return URL.createObjectURL(blob); // generates a temporary link
};

window.jsInterop.appendClass = function (id, className) {
    var el = document.getElementById(id);
    if (el) {
        el.classList.add(className);
    }
};

window.jsInterop.removeClass = function (id, className) {
    var el = document.getElementById(id);
    if (el) {
        el.classList.remove(className);
    }
};

// SweetAlert Helpers
window.showSwalLoading = (title, message) => {
    Swal.fire({
        title: title || 'Processing Request',
        html: `<b class="swal-text-custom">${message || 'Please wait while we complete your request...'}</b>`,
        background: '#ffffff',
        allowOutsideClick: false,
        allowEscapeKey: false,
        showConfirmButton: false,
        didOpen: () => {
            Swal.showLoading();
        },
        customClass: {
            popup: 'swal-loading-popup',
            title: 'swal-title-custom',
            htmlContainer: 'swal-text-custom'
        }
    });
};

window.showSwalSuccess = (title) => {
    Swal.fire({
        position: 'center',
        icon: 'success',
        title: title,
        showConfirmButton: false,
        timer: 2000,
        background: '#ffffff',
        customClass: {
            title: 'swal-title-custom',
            popup: 'swal-success-popup'
        }
    });
};

window.showSwalError = (title, message) => {
    Swal.fire({
        icon: 'error',
        title: title || 'Error',
        text: message,
        customClass: {
            popup: 'swal-error-popup'
        }
    });
};

window.showSwalWarning = (title, text) => {
    Swal.fire({
        title: title,
        html: text,
        icon: 'warning',
        confirmButtonText: 'OK',
        customClass: {
            popup: 'swal-warning-popup',
            title: 'swal-title-custom',
            text: 'swal-text-custom'
        }
    });
};

window.showSwalValidationWarning = (title, htmlMessage) => {
    Swal.fire({
        title: title,
        html: htmlMessage,
        icon: 'warning',
        confirmButtonText: 'OK',
        customClass: {
            popup: 'swal-warning-popup',
            title: 'swal-title-custom',
            htmlContainer: 'swal-html-custom'
        }
    });
};

window.showSwalGeneralLoading = (title, message) => {
    Swal.fire({
        title: title || 'Loading',
        html: `<b class="swal-text-custom">${message || 'Please wait...'}</b>`,
        background: '#ffffff',
        allowOutsideClick: false,
        allowEscapeKey: false,
        showConfirmButton: false,
        didOpen: () => {
            Swal.showLoading();
        },
        customClass: {
            popup: 'swal-loading-popup',
            title: 'swal-title-custom',
            htmlContainer: 'swal-text-custom'
        }
    });
};

window.showSwalConfirm = (title, htmlMessage) => {
    return Swal.fire({
        title: title,
        html: htmlMessage,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, proceed',
        cancelButtonText: 'Cancel',
        reverseButtons: true,
        focusCancel: true,
        customClass: {
            popup: 'swal-warning-popup',
            title: 'swal-title-custom',
            htmlContainer: 'swal-html-custom',
            confirmButton: 'swal-confirm-button',
            cancelButton: 'swal-cancel-button'
        }
    }).then((result) => {
        return result.isConfirmed;
    });
};

window.downloadFile = (filename, contentType, base64) => {
    const link = document.createElement('a');
    link.href = `data:${contentType};base64,${base64}`;
    link.download = filename;
    link.click();
};