// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
  window.addEventListener("load", function () {
    document.getElementById("loadingScreen").classList.add("visually-hidden");
    document.getElementById("mainContent").classList.remove("visually-hidden");
  });
  /**
  on scroll ->
    if title  ->
      :set:sticky
    if btn    -> 
      :show:btn
 */
  // var tableResponive = document.getElementsByClassName("table-responsive")[0];
  // var filterToolbar = document.getElementsByClassName("filter-toolbar")[0];
  var titleSection = document.getElementsByClassName("sectionTitle")[0];
  // var actionButtons = document.getElementsByClassName("actionButtons")[0];
  var goToTopButton = document.getElementById("goToTop");

  if (titleSection != null) {
    // sectionTitle container-fluid shadow-sm mb-3
    titleSection.classList.add("container-fluid");
    titleSection.classList.add("shadow-sm");
    titleSection.classList.add("mb-3");
  }

  window.onscroll = function () {
    // STICKY TITLE
    if (titleSection != null) {
      // var scrollSize =
      //   document.body.scrollTop || document.documentElement.scrollTop;
      // console.log(scrollSize);
      // if (scrollSize >= 50) {
      //   titleSection.classList.add("sticky-top");
      // } else {
      //   titleSection.classList.remove("sticky-top");
      // }

      // Event listeners
      window.addEventListener("scroll", toggleStickyTitle, { passive: true });
      if (goToTopButton != null) {
        goToTopButton.addEventListener("click", scrollToTop);
      }
      // Initialize
      toggleStickyTitle();
    }

    // GO to TOP
    if (goToTopButton != null) {
      if (
        document.body.scrollTop > 20 ||
        document.documentElement.scrollTop > 20
      ) {
        goToTopButton.style.display = "block";
      } else {
        goToTopButton.style.display = "none";
      }
      // When the user clicks on the button, scroll to the top of the document
      goToTopButton.onclick = function () {
        document.body.scrollTop = 0; // For Safari
        document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE, and Opera
      };
    }
  };

  // // -------------------Title section of page-------------------
  // // ** title + breadcrumbs
  // // var titleSection = document.getElementById("sectionTitle");
  // var titleSection = document.getElementsByClassName("sectionTitle")[0];

  // if (titleSection != null) {
  //   window.onscroll = function () {
  //     if (
  //       document.body.scrollTop > 20 ||
  //       document.documentElement.scrollTop > 20
  //     ) {
  //       console.log("Scrolled greater than 20");
  //       titleSection.classList.add("sticky-top");
  //       titleSection.style.width = "100vw";
  //       titleSection.style.paddingTop = "4em";
  //       titleSection.style.boxShadow = "0 2px 12px rgba(0, 0, 0, 0.1)";
  //     } else {
  //       titleSection.classList.remove("sticky-top");
  //       titleSection.style.paddingTop = "0em";
  //       titleSection.style.boxShadow = "none";
  //     }
  //   };
  // }

  // // ------------------- goToTop button implementation ---------------
  // var goToTopButton = document.getElementById("goToTop");

  // if (goToTopButton != null) {
  //   // When the user scrolls down 20px from the top of the document, show the button
  //   window.onscroll = function () {
  //     if (
  //       document.body.scrollTop > 20 ||
  //       document.documentElement.scrollTop > 20
  //     ) {
  //       goToTopButton.style.display = "block";
  //     } else {
  //       goToTopButton.style.display = "none";
  //     }
  //   };

  //   // When the user clicks on the button, scroll to the top of the document
  //   goToTopButton.onclick = function () {
  //     document.body.scrollTop = 0; // For Safari
  //     document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE, and Opera
  //   };
  // }

  // Cross-browser scroll position detection
  function getScrollPosition() {
    return (
      window.pageYOffset ||
      document.documentElement.scrollTop ||
      document.body.scrollTop ||
      0
    );
  }

  // Toggle sticky class based on scroll position
  function toggleStickyTitle() {
    var scrollSize = getScrollPosition();

    // if (scrollSize >= 50 && scrollSize < 75) {
    //   titleSection.classList.add("sticky-top");
    //   if (goToTopButton != null) {
    //     goToTopButton.style.display = "block";
    //   }
    //   if (filterToolbar != null) {
    //     filterToolbar.classList.add("sticky-top");
    //     filterToolbar.classList.add("shadow-sm");
    //     filterToolbar.style.paddingTop = "0em";
    //     filterToolbar.style.paddingBottom = "2em";
    //     filterToolbar.style.background =
    //       "linear-gradient(to right, #ffffffba, #f6f6f6af)";
    //     filterToolbar.style.transition =
    //       "background 0.3s, backdrop-filter 0.3s";
    //     filterToolbar.style.backdropFilter = "blur(4px)";
    //   }
    // }
    // //   // >=  75
    // else if (scrollSize >= 75 && scrollSize < 100) {
    //   if (filterToolbar != null) {
    //     filterToolbar.style.paddingTop = "3em";
    //   }
    // }
    // //   // >= 100
    // else if (scrollSize >= 100 && scrollSize < 120) {
    //   if (filterToolbar != null) {
    //     filterToolbar.style.paddingTop = "8em";
    //   }
    // } else if (scrollSize >= 120) {
    //   if (filterToolbar != null) {
    //     filterToolbar.style.paddingTop = "8em";
    //   }
    // } else {
    //   titleSection.classList.remove("sticky-top");
    //   if (goToTopButton != null) {
    //     goToTopButton.style.display = "none";
    //   }
    //   if (filterToolbar != null) {
    //     filterToolbar.classList.remove("sticky-top");
    //     filterToolbar.style.paddingTop = "0em";
    //     filterToolbar.style.paddingBottom = "1em";
    //     filterToolbar.style.background = "none";
    //     // "linear-gradient(to right, #ffffffba, #f6f6f6af)";
    //     filterToolbar.style.transition = "none";
    //     // "background 0.3s, backdrop-filter 0.3s";
    //     filterToolbar.style.backdropFilter = "none"; //"blur(4px)";
    //   }
    // }
  }

  // Scroll to top function
  function scrollToTop() {
    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  }
});
