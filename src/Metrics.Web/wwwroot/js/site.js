// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
  /**
  on scroll ->
    if title  ->
      :set:sticky
    if btn    -> 
      :show:btn
 */
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
      goToTopButton.addEventListener("click", scrollToTop);
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

    if (scrollSize >= 50) {
      titleSection.classList.add("sticky-top");
      goToTopButton.style.display = "block";
    } else {
      titleSection.classList.remove("sticky-top");
      goToTopButton.style.display = "none";
    }
  }

  // Scroll to top function
  function scrollToTop() {
    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  }
});
