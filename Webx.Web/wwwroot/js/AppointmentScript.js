
function changeStore(comboBoxComponent) {
    debugger;
    var storeId = comboBoxComponent.value;

    var url = "/Appointment/Index?storeId=" + storeId;

    window.location.href = url;
   
}

