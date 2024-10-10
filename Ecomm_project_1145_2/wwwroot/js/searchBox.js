$('#txtsearch').keyup(function () {
    var typeValues = $(this).val();
    $('div').each(function () {
        if ($(this).text().search(new RegExp(typeValues, "i")) < 0){
        $(this).fadeOute();
    }
    else{
        $(this).show();
    }
    })
})