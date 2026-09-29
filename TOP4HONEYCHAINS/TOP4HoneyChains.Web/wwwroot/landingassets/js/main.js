jQuery(document).ready(function( $ ) {

  // Hide preloader when page is loaded
  $(window).on('load', function() {
    $('#preloader').fadeOut('slow', function() {
      $(this).remove();
    });
  });
  
  // Fallback: hide preloader after 3 seconds
  setTimeout(function() {
    $('#preloader').fadeOut('slow');
  }, 3000);

  // Language switching functionality
  window.toggleLanguageMenu = function(event) {
    if (event) {
      event.preventDefault();
      event.stopPropagation();
    }
    
    const languageSwitch = document.querySelector('.language-switch');
    if (!languageSwitch) return;
    
    const isOpen = languageSwitch.classList.contains('open');
    
    if (isOpen) {
      languageSwitch.classList.remove('open');
    } else {
      languageSwitch.classList.add('open');
    }
  };

  window.switchLanguage = function(lang) {
    // Close the dropdown menu
    const languageSwitch = document.querySelector('.language-switch');
    if (languageSwitch) {
      languageSwitch.classList.remove('open');
    }
    
    // Update HTML lang attribute
    document.getElementById('html-root').className = 'lang-' + lang;
    document.getElementById('html-root').setAttribute('lang', lang);
    
    // Update page title
    const titleElement = document.querySelector('title');
    if (titleElement) {
      titleElement.textContent = titleElement.getAttribute('data-' + lang) || titleElement.textContent;
    }
    
    // Update meta descriptions
    const metaKeywords = document.querySelector('meta[name="keywords"]');
    const metaDescription = document.querySelector('meta[name="description"]');
    if (metaKeywords) {
      metaKeywords.setAttribute('content', metaKeywords.getAttribute('data-' + lang) || metaKeywords.getAttribute('content'));
    }
    if (metaDescription) {
      metaDescription.setAttribute('content', metaDescription.getAttribute('data-' + lang) || metaDescription.getAttribute('content'));
    }
    
    // Update all elements with data-lang attributes
    document.querySelectorAll('[data-tr], [data-en], [data-es], [data-pl], [data-mt], [data-lv]').forEach(function(element) {
      const text = element.getAttribute('data-' + lang);
      if (text) {
        element.textContent = text;
      }
    });
    
    // Update language button
    const currentFlag = document.getElementById('current-flag');
    const currentLang = document.getElementById('current-lang');
    
    if (lang === 'en') {
      currentFlag.className = 'flag flag-en';
      currentLang.textContent = 'EN';
    } else if (lang === 'tr') {
      currentFlag.className = 'flag flag-tr';
      currentLang.textContent = 'TR';
    } else if (lang === 'es') {
      currentFlag.className = 'flag flag-es';
      currentLang.textContent = 'ES';
    } else if (lang === 'pl') {
      currentFlag.className = 'flag flag-pl';
      currentLang.textContent = 'PL';
    } else if (lang === 'mt') {
      currentFlag.className = 'flag flag-mt';
      currentLang.textContent = 'MT';
    } else if (lang === 'lv') {
      currentFlag.className = 'flag flag-lv';
      currentLang.textContent = 'LV';
    }
    
    // Update language options
    document.querySelectorAll('.language-option').forEach(function(option) {
      option.classList.remove('active');
    });
    
    // Mark the selected option as active
    const selectedOption = document.querySelector(`.language-option[onclick="switchLanguage('${lang}')"]`);
    if (selectedOption) {
      selectedOption.classList.add('active');
    }
    
    // Store language preference
    localStorage.setItem('preferred-language', lang);
  };

  // Load saved language preference
  const savedLang = localStorage.getItem('preferred-language') || 'en';
  switchLanguage(savedLang);

  // Close dropdown when clicking outside
  document.addEventListener('click', function(event) {
    const languageSwitch = document.querySelector('.language-switch');
    
    if (languageSwitch && !languageSwitch.contains(event.target)) {
      languageSwitch.classList.remove('open');
    }
  });

  // Back to top button
  $(window).scroll(function() {
    if ($(this).scrollTop() > 100) {
      $('.back-to-top').fadeIn('slow');
    } else {
      $('.back-to-top').fadeOut('slow');
    }
  });
  $('.back-to-top').click(function(){
    $('html, body').animate({scrollTop : 0},1500, 'easeInOutExpo');
    return false;
  });

  // Header fixed on scroll
  $(window).scroll(function() {
    if ($(this).scrollTop() > 100) {
      $('#header').addClass('header-scrolled');
    } else {
      $('#header').removeClass('header-scrolled');
    }
  });

  if ($(window).scrollTop() > 100) {
    $('#header').addClass('header-scrolled');
  }

  // Real view height for mobile devices
  if (window.matchMedia("(max-width: 767px)").matches) {
    $('#intro').css({ height: $(window).height() });
  }

  // Initiate the wowjs animation library
  new WOW().init();

  // Initialize Venobox
  $('.venobox').venobox({
    bgcolor: '',
    overlayColor: 'rgba(6, 12, 34, 0.85)',
    closeBackground: '',
    closeColor: '#fff'
  });

  // Initiate superfish on nav menu
  $('.nav-menu').superfish({
    animation: {
      opacity: 'show'
    },
    speed: 400
  });

  // Mobile Navigation
  if ($('#nav-menu-container').length) {
    var $mobile_nav = $('#nav-menu-container').clone().prop({
      id: 'mobile-nav'
    });
    $mobile_nav.find('> ul').attr({
      'class': '',
      'id': ''
    });
    $('body').append($mobile_nav);
    $('body').prepend('<button type="button" id="mobile-nav-toggle" aria-controls="mobile-nav" aria-expanded="false" aria-label="Menüyü Aç/Kapat"><i class="fa fa-bars"></i></button>');
    $('body').append('<div id="mobile-body-overly"></div>');
    $('#mobile-nav').attr({ role: 'navigation', 'aria-hidden': true });
    $('#mobile-nav').find('.menu-has-children').prepend('<i class="fa fa-chevron-down"></i>');
    
    // Re-attach event handlers for mobile language switcher
    $('#mobile-nav .language-btn').off('click').on('click', function(e) {
      toggleLanguageMenu(e);
    });
    
    $('#mobile-nav .language-option').off('click').on('click', function(e) {
      const lang = $(this).attr('onclick').match(/switchLanguage\('(\w+)'\)/)[1];
      switchLanguage(lang);
    });

    $(document).on('click', '.menu-has-children i', function(e) {
      $(this).next().toggleClass('menu-item-active');
      $(this).nextAll('ul').eq(0).slideToggle();
      $(this).toggleClass("fa-chevron-up fa-chevron-down");
    });

    $(document).on('click', '#mobile-nav-toggle', function(e) {
      var isOpen = !$('body').hasClass('mobile-nav-active');
      $('body').toggleClass('mobile-nav-active');
      $('#mobile-nav-toggle i').toggleClass('fa-times fa-bars');
      $('#mobile-body-overly').toggle();
      $('#mobile-nav').attr('aria-hidden', !isOpen);
      $('#mobile-nav-toggle').attr('aria-expanded', isOpen);
    });

    $(document).click(function(e) {
      var container = $("#mobile-nav, #mobile-nav-toggle");
      if (!container.is(e.target) && container.has(e.target).length === 0) {
        if ($('body').hasClass('mobile-nav-active')) {
          $('body').removeClass('mobile-nav-active');
          $('#mobile-nav-toggle i').toggleClass('fa-times fa-bars');
          $('#mobile-body-overly').fadeOut();
          $('#mobile-nav').attr('aria-hidden', true);
          $('#mobile-nav-toggle').attr('aria-expanded', false);
        }
      }
    });
  } else if ($("#mobile-nav, #mobile-nav-toggle").length) {
    $("#mobile-nav, #mobile-nav-toggle").hide();
  }

  // Smooth scroll for the menu and links with .scrollto classes
  $('.nav-menu a, #mobile-nav a, .scrollto').on('click', function() {
    if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
      var target = $(this.hash);
      if (target.length) {
        var top_space = 0;

        if ($('#header').length) {
          top_space = $('#header').outerHeight();

          if( ! $('#header').hasClass('header-fixed') ) {
            top_space = top_space - 20;
          }
        }

        $('html, body').animate({
          scrollTop: target.offset().top - top_space
        }, 1500, 'easeInOutExpo');

        if ($(this).parents('.nav-menu').length) {
          $('.nav-menu .menu-active').removeClass('menu-active');
          $(this).closest('li').addClass('menu-active');
        }

        if ($('body').hasClass('mobile-nav-active')) {
          $('body').removeClass('mobile-nav-active');
          $('#mobile-nav-toggle i').toggleClass('fa-times fa-bars');
          $('#mobile-body-overly').fadeOut();
        }
        return false;
      }
    }
  });

  // Gallery carousel (uses the Owl Carousel library)
  $(".gallery-carousel").owlCarousel({
    autoplay: true,
    dots: true,
    loop: true,
    center:true,
    responsive: { 0: { items: 1 }, 768: { items: 3 }, 992: { items: 4 }, 1200: {items: 5}
    }
  });

  // Buy tickets select the ticket type on click
  $('#buy-ticket-modal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var ticketType = button.data('ticket-type');
    var modal = $(this);
    modal.find('#ticket-type').val(ticketType);
  })

// custom code

  // Countdown Timer for Workshop (January 12, 2026 00:00:00)
  function updateCountdown() {
    const workshopDate = new Date('2026-01-12T00:00:00').getTime();
    const now = new Date().getTime();
    const distance = workshopDate - now;

    if (distance < 0) {
      $('#workshop-countdown').html('<p style="color: #f82249; font-size: 24px; font-weight: 700;">Workshop Has Started!</p>');
      return;
    }

    const days = Math.floor(distance / (1000 * 60 * 60 * 24));
    const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((distance % (1000 * 60)) / 1000);

    $('#days').text(String(days).padStart(2, '0'));
    $('#hours').text(String(hours).padStart(2, '0'));
    $('#minutes').text(String(minutes).padStart(2, '0'));
    $('#seconds').text(String(seconds).padStart(2, '0'));
  }

  // Update countdown every second
  if ($('#workshop-countdown').length) {
    updateCountdown();
    setInterval(updateCountdown, 1000);
  }

  // Smooth scroll for floating submit button
  $('.floating-submit-btn').on('click', function(e) {
    e.preventDefault();
    const target = $($(this).attr('href'));
    if (target.length) {
      $('html, body').animate({
        scrollTop: target.offset().top - $('#header').outerHeight()
      }, 1000, 'easeInOutExpo');
    }
  });

  // Hide floating button when in Call for Papers section
  $(window).scroll(function() {
    if ($('#callforpapers').length) {
      const callSection = $('#callforpapers');
      const scrollPos = $(window).scrollTop();
      const sectionTop = callSection.offset().top - 200;
      const sectionBottom = sectionTop + callSection.outerHeight();
      
      if (scrollPos >= sectionTop && scrollPos <= sectionBottom) {
        $('.floating-submit-btn').fadeOut();
      } else {
        $('.floating-submit-btn').fadeIn();
      }
    }
  });

});
