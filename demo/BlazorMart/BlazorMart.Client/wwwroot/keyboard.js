// Direct all keyboard input (e.g., from barcode scanner) to the search box
// This is so that users don't have to tap on the search box before scanning items
document.body.addEventListener('keydown', evt => {
    var searchInput = document.querySelector('input[type=search]');
    if (document.activeElement !== searchInput) {
        searchInput.value = '';
        searchInput.focus();
    }
});
