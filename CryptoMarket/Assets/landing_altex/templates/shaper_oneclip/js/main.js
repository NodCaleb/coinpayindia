/**
 * @package Helix3 Framework
 * @author JoomShaper http://www.joomshaper.com
 * @copyright Copyright (c) 2010 - 2015 JoomShaper
 * @license http://www.gnu.org/licenses/gpl-2.0.html GNU/GPLv2 or later
 */
jQuery(function ($) {

    var $body = $('body'),
            $wrapper = $('.body-innerwrapper'),
            $toggler = $('#offcanvas-toggler'),
            $close = $('.close-offcanvas'),
            $offCanvas = $('.offcanvas-menu');

    $toggler.on('click', function (event) {
        event.preventDefault();
        stopBubble(event);
        setTimeout(offCanvasShow, 50);
    });

    $close.on('click', function (event) {
        event.preventDefault();
        offCanvasClose();
    });

    var offCanvasShow = function () {
        $body.addClass('offcanvas');
        $wrapper.on('click', offCanvasClose);
        $close.on('click', offCanvasClose);
        $offCanvas.on('click', stopBubble);

    };

    var offCanvasClose = function () {
        $body.removeClass('offcanvas');
        $wrapper.off('click', offCanvasClose);
        $close.off('click', offCanvasClose);
        $offCanvas.off('click', stopBubble);
    };

    var stopBubble = function (e) {
        e.stopPropagation();
        return true;
    };

    $('<div class="offcanvas-overlay"></div>').insertBefore('.body-innerwrapper > .offcanvas-menu');

    $('.close-offcanvas, .offcanvas-overlay').on('click', function (event) {
        event.preventDefault();
        $('body').removeClass('offcanvas');
    });

    //Mega Menu
    $('.sp-megamenu-wrapper').parent().parent().css('position', 'static').parent().css('position', 'relative');
    $('.sp-menu-full').each(function () {
        $(this).parent().addClass('menu-justify');
    });

    $('.show-menu').click(function () {
        $('.show-menu').toggleClass('active');
    });

    var nav_collapse = $('.nav.menu');
    nav_collapse.click('li a', function () {
        offCanvasClose();
    });


    //Search
    var searchRow = $('.top-search-input-wrap').parent().closest('.row');
    $('.top-search-input-wrap').insertAfter(searchRow);

    $(".search-open-icon").on('click', function () {
        $(".top-search-input-wrap").slideDown(200);
        $(this).hide();
        $('.search-close-icon').show();
        $(".top-search-input-wrap").addClass('active');
    });

    $(".search-close-icon").on('click', function () {
        $(".top-search-input-wrap").slideUp(200);
        $(this).hide();
        $('.search-open-icon').show();
        $(".top-search-input-wrap").removeClass('active');
    });

    //Slideshow height
    var slideHeight = $('.applanding-static-slider').outerHeight(true);
    $('#sp-header:not(".menu-fixed")').css('top', slideHeight);

    $(".varition-advance .customNavigation, .varition-advance .owl-dots").wrapAll("<div class='container controller-wrapper' />");
    $(".varition-thumb .customNavigation, .varition-thumb .owl-dots").wrapAll("<div class='container controller' />");
    $(".varition-thumb .controller").wrapAll("<div class='controller-wrapper' />");

    // Add class menu-fixed when scroll
    var windowWidth = $(window).width();

    if ($('body').hasClass('home')) {
        var windowHeight = $(window).height() - 70;
    } else {
        var windowHeight = $('#sp-menu').offset().top;
    }
    ;


    var stickyNav = function () {
        var scrollTop = $(window).scrollTop();

        if (scrollTop > slideHeight) {
            $('#sp-header').removeClass('menu-fixed-out').addClass('menu-fixed');
            $('#sp-header').css('top', 0);
        } else
        {
            if ($('#sp-header').hasClass('menu-fixed'))
            {
                $('#sp-header').removeClass('menu-fixed').addClass('menu-fixed-out');
                $('#sp-header').css('top', slideHeight);
            }

        }

    };

    stickyNav();

    $(window).scroll(function () {
        stickyNav();
    });

    //menu for applanding
    if ($('body').hasClass('variation-applanding')) {
        $(window).on('scroll', function () {
            if ($(window).scrollTop() > slideHeight) {
                $('#sp-header').addClass('menu-fixed');
            } else {
                $('#sp-header').removeClass('menu-fixed');
            }
        });
    };


    // ******* Menu link ******** //
    var homeSectionId = $('#sp-page-builder > .page-content > section:first-child').attr('id');   // home section id

    //if first section hasn't id
    if (homeSectionId == undefined) {
        $('#sp-page-builder > .page-content > section:first-child').attr('id', 'first-section');
    }

    $('.sp-megamenu-wrapper ul, .nav.menu').find('li:not(".no-scroll")').each(function (i, el) {
        var $that = $(this),
                $anchor = $that.children('a'),
                url = $anchor.attr('href'),
                splitUrl = url.split('#');

        if ($that.hasClass('home')) {
            //alert(homeSectionId);
            if (homeSectionId) {
                $anchor.attr('href', oneClipUrl + '#' + homeSectionId);
            } else {
                $anchor.attr('href', oneClipUrl);
            }
        } else {
            if (typeof splitUrl !== undefined) {
                $anchor.attr('href', oneClipUrl + '#' + splitUrl[1]);
            }
            ;
        }
    });

    //onepage nav
    $('.sp-megamenu-parent, .nav.menu').onePageNav({
        currentClass: 'active',
        changeHash: false,
        scrollSpeed: 900,
        scrollOffset: 60,
        scrollThreshold: 0.5,
        filter: ':not(.no-scroll)'
    });


    //Screenshot slider
    if ($(".screenshot-slider").length) {
        
        $('.screenshot-slider').owlCarousel({
            stagePadding: 100,
            loop: true,
            center: true,
            margin: 30,
            nav: true,
            autoWidth: false,
            autoHeight: false,
            navText: ['<span class="fa fa-caret-left"></span>', '<span class="fa fa-caret-right"></span>'],
            autoplay: true,
            responsive: {
                0: {
                    items: 1,
                    margin: 15,
                    stagePadding: 30
                },
                480: {
                    items: 1,
                    margin: 30,
                    stagePadding: 110
                },
                600: {
                    items: 3
                },
                768: {
                    items: 3,
                    margin: 30,
                    stagePadding: 0
                },
                992: {
                    items: 3,
                    margin: 30,
                    stagePadding: 95
                },
                1199: {
                    items: 6
                }
            }
        });
    };

    //Tooltip
    $('[data-toggle="tooltip"]').tooltip();

    $(document).on('click', '.sp-rating .star', function (event) {
        event.preventDefault();

        var data = {
            'action': 'voting',
            'user_rating': $(this).data('number'),
            'id': $(this).closest('.post_rating').attr('id')
        };

        var request = {
            'option': 'com_ajax',
            'plugin': 'helix3',
            'data': data,
            'format': 'json'
        };

        $.ajax({
            type: 'POST',
            data: request,
            beforeSend: function () {
                $('.post_rating .ajax-loader').show();
            },
            success: function (response) {
                var data = $.parseJSON(response.data);

                $('.post_rating .ajax-loader').hide();

                if (data.status == 'invalid') {
                    $('.post_rating .voting-result').text('You have already rated this entry!').fadeIn('fast');
                } else if (data.status == 'false') {
                    $('.post_rating .voting-result').text('Somethings wrong here, try again!').fadeIn('fast');
                } else if (data.status == 'true') {
                    var rate = data.action;
                    $('.voting-symbol').find('.star').each(function (i) {
                        if (i < rate) {
                            $(".star").eq(-(i + 1)).addClass('active');
                        }
                    });

                    $('.post_rating .voting-result').text('Thank You!').fadeIn('fast');
                }

            },
            error: function () {
                $('.post_rating .ajax-loader').hide();
                $('.post_rating .voting-result').text('Failed to rate, try again!').fadeIn('fast');
            }
        });
    });

    //Pricing Table Hover
    if ($('.sppb-addon-pricing-table.simple .sppb-pricing-box').length > 0) {
        $('.sppb-addon-pricing-table.simple .sppb-pricing-box').on('mouseover', function () {
            $('.sppb-pricing-box.sppb-pricing-featured').removeClass('sppb-pricing-featured');
            $(this).addClass('sppb-pricing-featured');
        });
    }

    // testimonial pro
    $('.sppb-testimonial-pro.variation-multiple .sppb-item').each(function () {
        var next = $(this).next();
        if (!next.length) {
            next = $(this).siblings(':first');
        }
        next.children(':first-child').clone().appendTo($(this));

        if (next.next().length > 0) {
            next.next().children(':first-child').clone().appendTo($(this));
        } else {
            $(this).siblings(':first').children(':first-child').clone().appendTo($(this));
        }
    });

});