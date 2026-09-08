// El tamaño configurado es el máximo; se reduce cuando el texto no entra.
(() => {
    const aviso = document.getElementById("aviso");
    const panel = document.getElementById("panelPrincipal");
    function ajustarAviso() {
        const maximo = (Number(config.tamanoAviso) || 10) * window.innerWidth / 100;
        aviso.style.fontSize = maximo + "px";
        const ancho = aviso.clientWidth;
        if (!ancho) return;
        if (aviso.scrollWidth > ancho) {
            aviso.style.fontSize = Math.floor(maximo * ancho / aviso.scrollWidth) + "px";
        }
        // La espera se muestra un 20 % más chica que el tamaño ajustado.
        if (!aviso.querySelector("br")) aviso.style.fontSize = (parseFloat(aviso.style.fontSize) * 0.8) + "px";
    }
    new ResizeObserver(ajustarAviso).observe(panel);
    new MutationObserver(ajustarAviso).observe(aviso, { childList: true, characterData: true, subtree: true });
    window.addEventListener("resize", ajustarAviso);
    document.fonts.ready.then(ajustarAviso);
    ajustarAviso();
})();


