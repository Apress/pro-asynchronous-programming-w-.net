$(function () {
    // view model
    function viewModel() {
        var self = this;

        self.displayError = ko.observable(false);
        self.errorMessage = ko.observable("");

        // Actions
        self.action = ko.observableArray([]);
        self.displayActions = ko.computed(function () {
            return self.action() !== null && self.action().length !== 0;
        });
        self.selectedActionCount = ko.computed(function () {
            var count = 0;

            $.each(self.action(), function (idx, entity) {
                if (entity.foundWithSelectorsRun === true) {
                    count++;
                }
            });

            return count;
        });

        self.controllers = ko.observableArray([]);
        self.displayControllers = ko.computed(function () {
            return self.controllers() !== null && self.controllers().length !== 0;
        });

        self.route = {
            routeTemplate: ko.observable(""),
            data: ko.observableArray([])
        };
        self.displayRouteData = ko.computed(function () {
            return self.route.routeTemplate() !== "" || self.route.data().length !== 0;
        });

        self.routes = ko.observableArray([]);
        self.displayRoutes = ko.computed(function () {
            return self.routes().length !== 0;
        });

        self.realStatusCode = ko.observable("");
        self.displayStatusCode = ko.computed(function () {
            return self.realStatusCode() !== "";
        });
    };

    var model = new viewModel();

    function routeModel(routePOCO) {
        var self = this;

        self.serializeKeyValueEnumerable = function (data) {
            var retval = "";

            if (data !== null) {
                $.each(data, function (idx, entity) {
                    retval += "[" + entity.Key + ":" + entity.Value + "] ";
                })

                retval.trim();
            }

            return retval;
        }

        self.raw = routePOCO;
        self.routeTemplate = self.raw.RouteTemplate;
        self.picked = self.raw.Picked;
        self.handler = self.raw.Handler;
        self.defaults = ko.computed(function () {
            return self.serializeKeyValueEnumerable(self.raw.Defaults);
        });
        self.constraints = ko.computed(function () {
            return self.serializeKeyValueEnumerable(self.raw.Constraints);
        });
        self.dataTokens = ko.computed(function () {
            return self.serializeKeyValueEnumerable(self.raw.DataTokens);
        });
    }

    function controllerModel(controllerPOCO) {
        var self = this;

        self.name = controllerPOCO.ControllerName;
        self.picked = false;

        var longtype = controllerPOCO.ControllerType.split(",");

        self.typename = longtype[0].trim();
        self.assembly = longtype[1].trim();
        self.version = longtype[2].split("=")[1].trim();
        self.culture = longtype[3].split("=")[1].trim();
        self.token = longtype[4].split("=")[1].trim();
    }

    /// functions
    function updateModel(data) {
        reset();

        // update route data
        model.route.routeTemplate(data.RouteData.RouteTemplate);

        $.each(data.RouteData.Data, function (idx, entity) {
            model.route.data.push({ key: entity.Key, value: entity.Value });
        });

        // update routes
        $.each(data.Routes, function (idx, entity) {
            model.routes.push(new routeModel(entity));
        });

        // update controller
        if (data.hasOwnProperty("Controller") && data.Controller !== null) {
            $.each(data.Controller, function (idx, entity) {
                var newone = new controllerModel(entity);
                if (newone.name === data.SelectedController) {
                    newone.picked = true;
                }
                model.controllers.push(newone);
            });
        }

        // update action
        if (data.hasOwnProperty("Action") && data.Action !== null) {
            $.each(data.Action.ActionSelections, function (idx, entity) {
                var thisAction = {};

                var methods = "";
                $.each(entity.SupportedHttpMethods, function (n, m) {
                    methods = methods + " " + m.Method;
                });
                thisAction.methods = methods.trim();

                thisAction.actionName = entity.ActionName;

                var param = "";
                $.each(entity.Parameters, function (n, m) {
                    var p = m.ParameterName + ":" + m.ParameterTypeName;
                    param = param + " " + p;
                });
                thisAction.param = param;

                thisAction.foundByActionName = entity.FoundByActionName;
                thisAction.foundByActionNameWithRightVerb = entity.FoundByActionNameWithRightVerb;
                thisAction.foundByVerb = entity.FoundByVerb;
                thisAction.foundWithRightParam = entity.FoundWithRightParam;
                thisAction.foundWithSelectorsRun = entity.FoundWithSelectorsRun;

                model.action.push(thisAction);
            });
        }

        // update status
        if (data.RealHttpStatus !== null) {
            model.realStatusCode(data.RealHttpStatus);
        }
    }

    function reset() {
        model.action.removeAll();
        model.displayError(false);
        model.errorMessage("");
        model.route.routeTemplate("");
        model.route.data.removeAll();
        model.routes.removeAll();
        model.controllers.removeAll();
        model.realStatusCode("000");
    }

    function onSuccess(data) {
        updateModel(data);
    }

    function onFail(jqXHR, textStatus, errorThrown) {
        model.displayError(true);
        model.errorMessage(jqXHR.status + " " + errorThrown);
    }

    function onSend(event) {
        reset();

        $("#errorMsg").text("");

        $.ajax({
            url: $("#testRoute").val(),
            type: $("#testMethod").val(),
            beforeSend: function (xhr) {
                xhr.setRequestHeader("RouteInspecting", "true");
            }
        })
            .done(onSuccess)
            .error(onFail);
    }

    var url = window.location.protocol + "//" + window.location.host;

    $("#testRoute").val(url + "/api/values");
    $("#btnDetect").click(onSend);
    $("#btnClear").click(reset);

    ko.applyBindings(model);
});