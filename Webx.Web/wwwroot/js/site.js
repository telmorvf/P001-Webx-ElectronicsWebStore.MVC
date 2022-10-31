// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//Get Profile Picture

function getBackgroundProfilePicture() {

    $.ajax({
        url: '/Account/GetProfilePicturePath',
        type: 'Post',
        dataType: 'json',
        success: function (user) {
            console.log(user);
                        
            document.getElementById('profilePictureBackground').style.background = 'url(' + user.imageFullPath + ') no-repeat center/100%';            

        },
        error: function (ex) {
            console.log(ex);
        }
    });
}

//change profile picture

function changeProfilePicture(inputId) {


    var input = document.getElementById(inputId).files[0];
    var formData = new FormData();
    formData.append('file', input);

    $.ajax({
        url: '/Account/ChangeProfilePic?handler=file',
        type: 'Post',
        data: formData,
        cache: false,
        processData: false,
        contentType: false,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("XSRF-TOKEN",
                $('input:hidden[name="__RequestVerificationToken"]').val());
        },
        success: function (response) {

            window.location.reload(true);

        },
        error: function (ex) {
            console.log(ex);
        }
    });
}