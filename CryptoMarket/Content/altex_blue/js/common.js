$(function(){
	$(".nano").nanoScroller({alwaysVisible: true});
	//$(".markets-table-tbody").nanoScroller({alwaysVisible: true});
	$(".header-nav-icon").click(function(){
		$(".header-nav ul").slideToggle();
	});
	/*begin tabs*/
	$(".tabs-nav a").click(function(){
	    parents=$(this).parents(".tabs-nav");
	    tabsBlock=parents.parents(".tabs-block");
	    parents.find(".active").removeClass("active");
	    $(this).addClass("active");
	    paneActive=tabsBlock.find(".pane.active");
	    paneActiveNew=$($(this).attr("href"));    
	    paneActive.fadeOut(100, function() {                
	        paneActive.removeClass("active"); 
	        paneActiveNew.addClass("active");
	        paneActiveNew.fadeIn(100);
	        $(".markets-table-tbody").nanoScroller({alwaysVisible: true});
	    });                  
	    return false;
	});
	/*end tabs*/
	
});