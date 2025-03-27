(function ($) {
	
	"use strict";

	$(document).ready(function() {
		var srcdt = $('#ws_event_from');
		if(srcdt.length > 0) {
			var fnn = srcdt.datepicker( 'option', 'onSelect');
			srcdt.datepicker( 'option', 'onSelect', function(){fnn();
			var tpick = $("#ws_event_to");
				tpick.datepicker( "setDate", $(this).datepicker( 'getDate' ) );
				tpick.datepicker( "option", "minDate", $(this).datepicker( 'getDate' ) );
				tpick.datepicker( "option", "maxDate", '+2y' );
			});
		}

	});

}(jQuery));
