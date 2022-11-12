filterArray = [];
values = [];
desiredResultsPerPage = 12;
minimumPrice = 0;
maximumPrice = document.getElementById("maxRange").value;
activeDiv = "";


window.onload = onloadFunction(); 

function onloadFunction() {

    debugger;
    

    var categoryNameToBeDisplayed = document.getElementById("CurrentCategory");

    if (categoryNameToBeDisplayed.innerHTML === "") {
        categoryNameToBeDisplayed.innerHTML = "AllCategories";
    }

}


function AddtoCart(id) {
    debugger;

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



function onOverlayClick() {
    var dialog = document.getElementById("dialog").ej2_instances[0];
    dialog.hide();
}; 

function showProductDetails(productId) {
    debugger;
    var category = document.getElementById("CurrentCategory").innerHTML;
    let arrayString = JSON.stringify(filterArray); 

    $.ajax({
        url: '/Products/GetProductDetails/',
        type: 'GET',
        contentType: 'application/html',
        dataType:"html",
        data: { Id: productId, resultsPerPage: desiredResultsPerPage, category: category, minRange: minimumPrice, maxRange: maximumPrice, brandsFilter: arrayString },
        success: function (partialViewResult) {
            debugger;

            var content = $("#modalPartial").html(partialViewResult); 
            eval(content);
            
            $('#quick-view').modal('toggle'); 
        },
        error: function (ex) {
            console.log("error");
        }
    })
}

function testFunction() {
    console.log("entrou");
}


function closeViewModel() {
  
    $('#quick-view').modal('hide');
}


function ClearFilters() {


    filterArray.forEach(function (value) {
        debugger;
        var element = document.getElementById(value);
        element.checked = false;
    })

    filterArray.length = 0;

    $.ajax({
        url: '/Products/ClearFilters/',
        type: 'GET',
        data: { resultsPerPage: desiredResultsPerPage }
    }).done(function (partialViewResult) {
        $("#refPartial").html(partialViewResult);
    });

    var currentCategory = document.getElementById("CurrentCategory");
    var divIdName = currentCategory.innerHTML+ "id";
    if (divIdName != "" && divIdName != "AllCategoriesid") {
        var div = document.getElementById(divIdName);
        div.classList.remove("activeDiv");
    }
    currentCategory.innerHTML = "AllCategories";
}


function filterBrand(identifier) {

    var brandName = identifier.id.replace('Identifier', '');
    var category = document.getElementById("CurrentCategory").innerHTML;
    
    if (identifier.checked) {
        filterArray.push(identifier.id.replace(' ', ''));
    }
    else {

        let itemToRemove = filterArray.indexOf(identifier.id);

        if (itemToRemove > -1) {
            filterArray.splice(itemToRemove, 1);
        }
    }
    
    let arrayString = JSON.stringify(filterArray);

    $.ajax({
        url: '/Products/FilterBrand/',
        type: 'GET',
        data: { category: category, resultsPerPage: desiredResultsPerPage, minRange: minimumPrice, maxRange: maximumPrice ,brandsfilter: arrayString }
    }).done(function (partialViewResult) {
        $("#refPartial").html(partialViewResult);
    });
}



function changeResultsPerPage(resultsPerPage) { 


    desiredResultsPerPage = resultsPerPage;
    var stringArrayToSend;
    var currentCategory = document.getElementById('CurrentCategory').innerHTML;

    var arrayOfBrandsFilter = document.querySelectorAll("[id^='liIdentifier']");
    var brandsNamesArray = [];
    if(arrayOfBrandsFilter.length > 0){
        arrayOfBrandsFilter.forEach(function(value){
            brandsNamesArray.push(value.id.replace("liIdentifier", "")+"Identifier");
        });

        stringArrayToSend = JSON.stringify(brandsNamesArray);
    }          

    $.ajax({
        url: '/Products/ChangeResultsPerPage/',
        type: 'GET',
        data: { category: currentCategory, resultsPerPage: resultsPerPage,brandsfilter: stringArrayToSend}
    }).done(function(partialViewResult) {
        $("#refPartial").html(partialViewResult);          
    });  

}

function changeCategory(category) {
    debugger;
    let divid = category + "id";
    let div = document.getElementById(divid);
    div.classList.add('activeDiv');

    if (divid !== activeDiv) {
        if (activeDiv != "" && activeDiv != "AllCategoriesid") {
            let oldActiveDiv = document.getElementById(activeDiv);
            oldActiveDiv.classList.remove('activeDiv');
        }        
        activeDiv = divid;
    }

    $.ajax({
        url: '/Products/ChangeCategory/',
        type: 'GET',
        data: { category: category }
    }).done(function (partialViewResult) {

        $("#refPartial").html(partialViewResult);       

        filterArray.forEach(function (value) {
            
            var element = document.getElementById(value);
            element.checked = false;

        })

        filterArray.length = 0;

    });
}

const rangeInput = document.querySelectorAll(".range-input input"),
priceInput = document.querySelectorAll(".price-input input"),
progress = document.querySelector(".slider .progress");

let priceGap = 10;
            
rangeInput.forEach(input => {
    input.addEventListener("input", e => {

        let minVal = parseInt(rangeInput[0].value),
            maxVal = parseInt(rangeInput[1].value);

        if (maxVal - minVal < priceGap) {
            if (e.target.className === "range-min") {
                rangeInput[0].value = maxVal - priceGap;
            }
            else {
                rangeInput[1].value = minVal + priceGap;
            }

        } else {
            priceInput[0].value = minVal;
            priceInput[1].value = maxVal;
            progress.style.left = (minVal / rangeInput[0].max) * 100 + "%";
            progress.style.right = 100 - (maxVal / rangeInput[1].max) * 100 + "%";
        }

    })

    input.addEventListener("change", p => {
        debugger;
        let minVal = parseInt(rangeInput[0].value),
            maxVal = parseInt(rangeInput[1].value);

        minimumPrice = minVal;
        maximumPrice = maxVal;

        var currentCategory = document.getElementById('CurrentCategory').innerHTML;
        let brandsfilter = JSON.stringify(filterArray);
       
        $.ajax({
            url: '/Products/ChangePriceRange/',
            type: 'GET',
            data: { category: currentCategory,resultsPerPage: desiredResultsPerPage,minRange:minVal,maxRange:maxVal ,brandsFilter: brandsfilter }
        }).done(function (partialViewResult) {

            $("#refPartial").html(partialViewResult);
        });

    })
})         