//Primera
$(document).ready(function () {
    // Muestra y oculta los menús
    $('#menu-altern ul li:has(ul)').click( //función al hace click en un "li" que tiene una "ul"
        function () {
            $(this).find('ul').slideToggle();
        });
});
//Segunda
$(document).ready(function () {
    // Muestra y oculta los menús
    $('#menu-altern ul li:has(ul)').hover( //función al pasar el ratón por encima de un "li" que tiene una "ul"
        function (e) //Primera función-->ratón por encima
        {
            $(this).find('ul').fadeIn();
        },
        function (e) //Cuando el ratón deja de estar encima.
        {
            $(this).find('ul').fadeOut();
        }
    );
});