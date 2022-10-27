(function ($) {
  /*==========================
    service Worker js
    ==========================*/
  $(window).on('load', function () {
    'use strict';
    if ('serviceWorker' in navigator) {
      navigator.serviceWorker
        .register('sw.js');
    }
  });

  /*==========================
    add to home screen Btn js
    ==========================*/
  let deferredPrompt;

  window.addEventListener('beforeinstallprompt', (e) => {
    deferredPrompt = e;
  });

  const installApp = document.getElementById('installApp');

  installApp.addEventListener('click', async () => {
    if (deferredPrompt !== null) {
      deferredPrompt.prompt();
      const {
        outcome
      } = await deferredPrompt.userChoice;
      if (outcome === 'accepted') {
        deferredPrompt = null;
      }
    }
  });
})(jQuery);