(function ($) {
	
    "use strict";

    $(document).ready(function() {

        // Comments
        $(".commentlist li").addClass("panel panel-default");
        $(".comment-reply-link").addClass("btn btn-default");

        // Forms
        $('select, input[type=text], input[type=email], input[type=password], textarea').addClass('form-control');
        $('input[type=submit]').addClass('btn btn-primary');

        // You can put your own code in here
        
        $("#searchform .search").on("click", function(e)
        {
            $("#s").focus();
        });
        
        /* mappa sx - tutti i musei */
        if ($('#homepage-map').length>0)
        {
            /* gestione mappa home page */
            var map;
            map = new GMaps({
              div: '#homepage-map',
              lat: 43.709545,
              lng: 10.399888,
              clickableIcons: false,
              disableDefaultUI: true,
              styles: 
              [
                {
                    "featureType": "administrative",
                    "elementType": "all",
                    "stylers": [
                        {
                            "saturation": "-100"
                        }
                    ]
                },
                {
                    "featureType": "administrative.province",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "off"
                        }
                    ]
                },
                {
                    "featureType": "landscape",
                    "elementType": "all",
                    "stylers": [
                        {
                            "saturation": -100
                        },
                        {
                            "lightness": 65
                        },
                        {
                            "visibility": "on"
                        }
                    ]
                },
                {
                    "featureType": "poi",
                    "elementType": "all",
                    "stylers": [
                        {
                            "saturation": -100
                        },
                        {
                            "lightness": "50"
                        },
                        {
                            "visibility": "simplified"
                        }
                    ]
                },
                {
                    "featureType": "road",
                    "elementType": "all",
                    "stylers": [
                        {
                            "saturation": "-100"
                        }
                    ]
                },
                {
                    "featureType": "road.highway",
                    "elementType": "all",
                    "stylers": [
                        {
                            "visibility": "simplified"
                        }
                    ]
                },
                {
                    "featureType": "road.arterial",
                    "elementType": "all",
                    "stylers": [
                        {
                            "lightness": "30"
                        }
                    ]
                },
                {
                    "featureType": "road.local",
                    "elementType": "all",
                    "stylers": [
                        {
                            "lightness": "40"
                        }
                    ]
                },
                {
                    "featureType": "transit",
                    "elementType": "all",
                    "stylers": [
                        {
                            "saturation": -100
                        },
                        {
                            "visibility": "simplified"
                        }
                    ]
                },
                {
                    "featureType": "water",
                    "elementType": "geometry",
                    "stylers": [
                        {
                            "hue": "#ffff00"
                        },
                        {
                            "lightness": -25
                        },
                        {
                            "saturation": -97
                        }
                    ]
                },
                {
                    "featureType": "water",
                    "elementType": "labels",
                    "stylers": [
                        {
                            "lightness": -25
                        },
                        {
                            "saturation": -100
                        }
                    ]
                },
                {
                    "featureType": "poi",
                    "elementType": "labels",
                    "stylers": [
                      { "visibility": "off" }
                    ]
                }
              ]
            });


            $(map_markers).each(function(i,e)
            {
                
                var icon={        
                    path: "M.5,1h0S16.8-23.9,16.8-32.1C16.7-44.1,8.5-49.4.5-49.4s-16.3,5.3-16.3,17.3C-15.8-23.9.4,1,.5,1ZM-5.2-32.8A5.59,5.59,0,0,1,.4-38.4,5.59,5.59,0,0,1,6-32.8,5.59,5.59,0,0,1,.4-27.2,5.53,5.53,0,0,1-5.2-32.8Z",
                    fillColor: map_markers[i].color.length>0 ? map_markers[i].color : '#000000',
                    fillOpacity: 1,
                    anchor: new google.maps.Point(0,0),
                    strokeWeight: 0,
                    class: map_markers[i].museum_class
                };

                map_markers[i].size=new google.maps.Size(20, 32);
                map_markers[i].icon=icon;
            });

            map.addMarkers(map_markers);

            map.fitZoom();
        }
    });

    
    $('.grey-box, li.evento, .container-category .articles li').matchHeight();
        
    $("body.single #sidebar").css('padding-top', $('article header').height()+'px');
    
    $('#sidebar').css("height", $('.article-list').height()+'px');
    
    // ----------------------------------------------
    // # Search
    // ----------------------------------------------

    $('.fa-search').on('click', function() {
        $('.search').fadeIn(500, function() {
          $(this).toggleClass('search-toggle');
        });     
    });

    $('.search-close').on('click', function() {
        $('.search').fadeOut(500, function() {
            $(this).removeClass('search-toggle');
        }); 
    });
    
    // ----------------------------------------------
    // # Dropdown Menu Animation 
    // ----------------------------------------------
    $('.dropdown').on('show.bs.dropdown', function(e)
    {
            $(this).find('.dropdown-menu').first().stop(true, true).slideDown(300);
    });
    
    $('.dropdown').on('hide.bs.dropdown', function(e)
    {
            $(this).find('.dropdown-menu').first().stop(true, true).slideUp(300);
    });


    if ($('#carousel-slideshow-home').length>0)
    {
        $('#carousel-slideshow-home .carousel-inner .item').each(function(i,e)
        {
            var url=$(this).data('url');
            if (url.length>0)
            {
                $(this).css('cursor','pointer');
                
                $(e).on('click', function(e)
                {
                    var url=$(this).data('url');
                    
                    $(this).css('cursor','pointer');
                    location.href=url;
                });
            }
        });
    }


}(jQuery));
