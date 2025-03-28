

// 當頁面加載完成後，隱藏 loading 動畫並顯示內容
document.addEventListener("DOMContentLoaded", function () {
    var loadingOverlay = document.getElementById("loadingOverlay");
    var content = document.querySelector(".content");

    // 隱藏 loading 動畫並顯示內容
    loadingOverlay.style.display = "none";
    content.style.display = "block";
});

//彈窗
/*
    window.onload = function() {
        var MessageModal1 = new bootstrap.Modal(document.getElementById('mModal1'), {
        keyboard: false  
        });
        MessageModal1.show();  
    };
    */
//暫時用sessionStorage避免按掉後廣告徹底消失，正式應該使用localStorage.
window.onload = function () {
    var hideAd = sessionStorage.getItem('hideAd');

    if (!hideAd) {
        var MessageModal1 = new bootstrap.Modal(document.getElementById('mModal1'), { keyboard: false });
        MessageModal1.show();
    }
    document.getElementById('flexCheckIndeterminate').addEventListener('change', function () {
        if (this.checked) {
            sessionStorage.setItem('hideAd', 'true');
        }
        else { sessionStorage.removeItem('hideAd'); }

    });
}

//加載
