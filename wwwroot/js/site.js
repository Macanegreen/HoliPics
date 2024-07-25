// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//document.addEventListener("DOMContentLoaded", function () {
//    var lazyloadImages;

//    if ("IntersectionObserver" in window) {
//        lazyloadImages = document.querySelectorAll(".lazy");
//        var imageObserver = new IntersectionObserver(function (entries, observer) {
//            entries.forEach(function (entry) {
//                if (entry.isIntersecting) {
//                    var image = entry.target;
//                    image.src = image.dataset.src;
//                    image.classList.remove("lazy");
//                    imageObserver.unobserve(image);
//                }
//            });
//        }, {
//            root: document.querySelector("#img-container"),
//            rootMargin: "0px 0px 800px 0px"
//        });

//        lazyloadImages.forEach(function (image) {
//            imageObserver.observe(image);
//        });
//    } else {
        
//    }
//})