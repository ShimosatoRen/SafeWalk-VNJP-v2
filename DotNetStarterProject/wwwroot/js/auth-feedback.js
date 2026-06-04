document.addEventListener('DOMContentLoaded', function () {
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            // jQuery Validation が有効な場合、バリデーションをチェック
            if (typeof $ !== 'undefined') {
                const $form = $(form);
                if ($form.data('validator')) {
                    if (!$form.valid()) {
                        return; // バリデーションエラーがある場合は何もしない
                    }
                }
            }

            const button = form.querySelector('button[type="submit"]');
            if (button && !button.disabled) {
                // 直後に無効化するとフォーム送信がキャンセルされるブラウザがあるため
                // setTimeout を使用して非同期で無効化
                setTimeout(() => {
                    button.disabled = true;
                    button.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>処理中...';
                }, 0);
            }
        });
    });
});
