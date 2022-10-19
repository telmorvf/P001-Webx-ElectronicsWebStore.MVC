// product image change js
$(".product-img ul li").on("click", function (e) {
  $(this).addClass("active").siblings(".active").removeClass("active");

  var path = $(this).find("a").attr("data-source");
  $(this)
    .parents()
    .find(".home-slider")
    .css("background-image", "url(" + path + ")");
});