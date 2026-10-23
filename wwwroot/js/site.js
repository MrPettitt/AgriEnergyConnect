// Agri-Energy Connect client-side helpers.

document.addEventListener("DOMContentLoaded", function () {
    // Auto-dismiss success banners (e.g. "Product added successfully") after a few seconds.
    document.querySelectorAll(".alert-dismissible.alert-success").forEach(function (alertEl) {
        setTimeout(function () {
            var alert = bootstrap.Alert.getOrCreateInstance(alertEl);
            alert.close();
        }, 5000);
    });
});

// Ask for confirmation before submitting any form that carries a data-confirm message.
document.addEventListener("submit", function (event) {
    var message = event.target.getAttribute && event.target.getAttribute("data-confirm");
    if (message && !window.confirm(message)) {
        event.preventDefault();
    }
});
