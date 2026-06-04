// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function googleTranslateElementInit() {
    new google.translate.TranslateElement({
        pageLanguage: 'ja',
        includedLanguages: 'ja,en,vi',
        autoDisplay: false
    }, 'google_translate_element');
}

// ページ読み込み完了後に言語選択を同期
window.addEventListener('load', function() {
    var savedLang = localStorage.getItem('preferred-lang') || 'ja';
    var languageSwitcher = document.getElementById('language-switcher');
    
    if (languageSwitcher) {
        languageSwitcher.value = savedLang;
        
        languageSwitcher.addEventListener('change', function() {
            var lang = this.value;
            var select = document.querySelector('.goog-te-combo');
            if (select) {
                select.value = lang;
                select.dispatchEvent(new Event('change'));
                localStorage.setItem('preferred-lang', lang);
            }
        });
    }
});
