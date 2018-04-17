/**
* @package Helix3 Framework
* @author JoomShaper http://www.joomshaper.com
* @copyright Copyright (c) 2010 - 2016 JoomShaper
* @license http://www.gnu.org/licenses/gpl-2.0.html GNU/GPLv2 or later
*/

jQuery(document).ready(function($){'use strict';
    
    // Full width Slideshow
    var $slideFullwidth = $('#slide-fullwidth');

    // Autoplay
    var $autoplay   = $slideFullwidth.attr('data-sppb-slide-ride');
    if ($autoplay == 'true') { var $autoplay = true; } else { var $autoplay = false};

    // controllers
    var $controllers   = $slideFullwidth.attr('data-sppb-slidefull-controllers');
    if ($controllers == 'true') { var $controllers = true; } else { var $controllers = false};
    

    $slideFullwidth.owlCarousel({
        margin: 0,
        loop: true,
        video:true,
        autoplay: $autoplay,
        animateIn: 'fadeIn',
        animateOut: 'fadeOut',
        autoplayHoverPause: true,
        autoplaySpeed: 1500,
        dotsContainer: '#sppb-custom-dots',
        responsive: {
            0: {
                items: 1
            },
            600: {
                items: 1
            },
            1000: {
                items: 1
            }
        },
        //dots: $controllers,
    });
    

    $('.sppbSlidePrev').click(function(){
        $slideFullwidth.trigger('prev.owl.carousel', [400]);
    });

    $('.sppbSlideNext').click(function(){
        $slideFullwidth.trigger('next.owl.carousel',[400]);
    });


});

