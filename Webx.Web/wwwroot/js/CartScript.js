productId = 0;

function updateCart(id, value) {

    debugger;

    $.ajax({
        url: '/Cart/UpdateCart/',
        type: 'GET',
        contentType: 'application/html',
        dataType: "html",
        data: {id:id,quantity:value},
        success: function (partial) {
            debugger;
            let content = $("#cartDetailsPartial").html(partial);
            eval(content);
        },
        error: function (ex) {
            console.log("error");
        }
    })

    if (value === "1") {
        $.ajax({
            url: '/Products/AddProduct/',
            type: 'GET',
            contentType: 'application/html',
            dataType: "html",
            data: { Id: id },
            success: function (value) {
                debugger;

                let content = $("#cartPartialDiv").html(value);
                eval(content);
            },
            error: function (ex) {
                debugger;

                console.log("error");
            }
        })
    }

    if (value === "-1") {

        $.ajax({
            url: '/Cart/RemoveProductFromDrowpDown/',
            type: 'GET',
            contentType: 'application/html',
            dataType: "html",
            data: { Id: id,quantity:value },
            success: function (value) {
                debugger;
                let content = $("#cartPartialDiv").html(value);
                eval(content);
            },
            error: function (ex) {
                debugger;

                console.log("error");
            }
        })
    }
        
}

function RemoveProduct(id) {
    debugger;
    $.ajax({
        url: '/Cart/GetProductDetails/',
        type: 'Post',       
        dataType: "json",
        data: { id: id },
        success: function (data) {
            debugger;
            console.log(data);
            productId = id;
            var dialog = document.getElementById("deleteDialog").ej2_instances[0];
            dialog.cssClass = 'e-fixed';
            dialog.content = "<p>Are you sure you want to delete <b style='color:var(--theme-color)'>" + data.name +"</b> from cart?</p><p style='font-size:10px'> After deleting this product, to get it back in your cart, you will have to add it again.</p>";
            dialog.show();            
        },
        error: function (ex) {
            console.log("error");
        }
    })

}

function onDeleteOverlayClick() {
    var dialog = document.getElementById("deleteDialog").ej2_instances[0];
    dialog.hide();
}

function ondlgDeleteClick() {

    $.ajax({
        url: '/Cart/RemoveProduct/',
        type: 'GET',
        contentType: 'application/html',
        dataType: "html",
        data: { id: productId },
        success: function (partial) {
            debugger;
            let content = $("#cartDetailsPartial").html(partial);
            eval(content);
            onDeleteOverlayClick();
        },
        error: function (ex) {
            console.log("error");
        }
    })


    $.ajax({
        url: '/Cart/RemoveProductFromDrowpDown/',
        type: 'GET',
        contentType: 'application/html',
        dataType: "html",
        data: { Id: productId, quantity: "total" },
        success: function (value) {
            debugger;
            let content = $("#cartPartialDiv").html(value);
            eval(content);
        },
        error: function (ex) {
            debugger;

            console.log("error");
        }
    })
}

function ondlgCancelClick() {
    onDeleteOverlayClick();
}


function onClearCartOverlayClick() {
    var dialog = document.getElementById("clearCartDialog").ej2_instances[0];
    dialog.hide();
}

function ClearCart() {
    debugger;

    var dialog = document.getElementById("clearCartDialog").ej2_instances[0];
    dialog.content = "<p>Are you sure you want to clear the cart? </p>"
    dialog.cssClass = 'e-fixed';
    dialog.show();    
}

function ondlgClearClick()
{
    debugger;

    $.ajax({
        url: '/Cart/ClearCart/',
        type: 'GET',
        contentType: 'application/html',
        dataType: "html", 
        success: function (partial) {
            debugger;
            let content = $("#cartDetailsPartial").html(partial);
            eval(content);            
            clearDropDownList();
            onClearCartOverlayClick();
        },
        error: function (ex) {
            console.log("error");
        }
    })    
}

function clearDropDownList() {
    $.ajax({
        url: '/Cart/UpdateDrowpDown/',
        type: 'GET',
        contentType: 'application/html',
        dataType: "html",
        success: function (value) {
            debugger;
            let content = $("#cartPartialDiv").html(value);
            eval(content);
        },
        error: function (ex) {
            debugger;

            console.log("error");
        }
    })
}

function ondlgCancelCartClick()
{
    onClearCartOverlayClick();
}