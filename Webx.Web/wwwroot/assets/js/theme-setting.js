/*=====================
      Color Picker
==========================*/
var color_picker1 = document.getElementById("ColorPicker1").value;
document.getElementById("ColorPicker1").onchange = function () {
  color_picker1 = this.value;
  document.body.style.setProperty("--theme-color", color_picker1);
};

/*========================
 Dark local storage setting js
 ==========================*/
$("#darkButton").on("click", function () {
  if ($("body").hasClass("light")) {
    var href = $("#color-link").attr("href");
    var split = href.split("/").pop().split(".").shift();
    $("body").removeClass("light");
    $("body").addClass("dark");
    $(this).text("Light");
    document
      .getElementById("color-link")
      .setAttribute("href", "assets/css/" + split + "_dark.css");
  } else {
    var href = $("#color-link").attr("href");
    var split = href.split("/").pop().split(".").shift().split("_").shift();
    $("body").removeClass("dark");
    $("body").addClass("light");
    $(this).text("Dark");
    document
      .getElementById("color-link")
      .setAttribute("href", "assets/css/" + split + ".css");
  }
});

/*========================
   RTL local storage setting js
   ==========================*/
$(".rtl-button").on("click", function () {
  if ($("body").hasClass("ltr")) {
    $("html").attr("dir", "rtl");
    $("body").removeClass("ltr");
    $("body").addClass("rtl");
    $(this).text("LTR");
    $("#rtl-link").attr("href", "assets/css/vendors/bootstrap.rtl.css");
  } else {
    $("html").attr("dir", "");
    $("body").removeClass("rtl");
    $(this).text("RTL");
    $("body").addClass("ltr");
    $("#rtl-link").attr("href", "assets/css/vendors/bootstrap.css");
  }
});