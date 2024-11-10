function updateLogo() {
    const logoImage = document.getElementById("logoImage");
    if (window.innerWidth <= 425) {
        logoImage.src = "/logo100x24.png";
    } else {
        logoImage.src = "/logo154x48.png";
    }
}

updateLogo();

window.addEventListener("resize", updateLogo);

function updateCollapse() {
    const ContactUs = document.getElementById("ContactUs");
    const TermsOfService = document.getElementById("TermsOfService");
    const navBarNav = ContactUs.parentElement;

    if (window.innerWidth <= 576) {
        navBarNav.insertBefore(TermsOfService, ContactUs);
    } else {
        navBarNav.insertBefore(ContactUs, TermsOfService);
    }
}

window.addEventListener("load", updateCollapse);
window.addEventListener("resize", updateCollapse);

if (document.getElementById('auth-sync')) {
    const root = ReactDOM.createRoot(document.getElementById('auth-sync'));
    root.render(<AuthSync />);
}