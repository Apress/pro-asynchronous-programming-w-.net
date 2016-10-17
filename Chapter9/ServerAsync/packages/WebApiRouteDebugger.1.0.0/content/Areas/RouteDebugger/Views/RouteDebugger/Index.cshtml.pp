<!DOCTYPE html>
<html lang="en">
<head>
    <title>ASP.NET Web API Route Debugger</title>
    <link href="~/Content/bootstrap.css" rel="stylesheet" />
    <link href="~/Areas/RouteDebugger/Content/rdstyle.css" rel="stylesheet" />
</head>
<body>
    <div class="navbar navbar-fixed-top">
        <div class="navbar-inner">
            <div class="container">
                <a class="brand" href="#">Route Debugger</a>
                <form class="navbar-form pull-left" target="_blank">
                    <select id="testMethod" class="span2">
                        <option>GET</option>
                        <option>POST</option>
                        <option>PUT</option>
                        <option>DELETE</option>
                    </select>
                    <div class="input-append">
                        <input id="testRoute" class="input-xxlarge" type="text" />
                        <input id="btnDetect" class="btn" type="button" value="Send" />
                        <input id="btnClear" class="btn" type="button" value="Clear" />
                    </div>
                </form>
            </div>
        </div>
    </div>
    <div class="container">
        <!-- Status -->
        <div class="row" data-bind="if: displayStatusCode">
            <h1 data-bind="text: realStatusCode"></h1>
        </div>
        <!-- Error Display -->
        <div class="row" data-bind="if: displayError">
            <p class="text-error" data-bind="text: errorMessage"></p>
        </div>
        <!-- Route Data -->
        <div class="row" data-bind="if: displayRouteData">
            <h1>Route Data</h1>
            <table class="table table-bordered table-condensed monospace">
                <thead>
                    <tr>
                        <th colspan="2">Template</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td colspan="2" data-bind="text: route.routeTemplate"></td>
                    </tr>
                </tbody>
                <thead>
                    <tr>
                        <th>Key</th>
                        <th>Value</th>
                    </tr>
                </thead>
                <tbody data-bind="foreach: route.data">
                    <tr>
                        <td data-bind="text: key"></td>
                        <td data-bind="text: value"></td>
                    </tr>
                </tbody>
            </table>
        </div>
        <!-- Routes -->
        <div class="row" data-bind="if: displayRoutes">
            <h1>Route selecting</h1>
            <table class="table table-bordered table-condensed monospace">
                <thead>
                    <tr>
                        <th>Template</th>
                        <th>Defaults</th>
                        <th>Constraints</th>
                        <th>Data Tokens</th>
                        <th>Handler</th>
                    </tr>
                </thead>
                <tbody data-bind="foreach: routes">
                    <tr data-bind="css: { success: picked==true }">
                        <td data-bind="text: routeTemplate"></td>
                        <td data-bind="text: defaults"></td>
                        <td data-bind="text: constraints"></td>
                        <td data-bind="text: dataTokens"></td>
                        <td data-bind="text: handler"></td>
                    </tr>
                </tbody>
            </table>
        </div>
        <!-- Controller -->
        <div class="row" data-bind="if: displayControllers">
            <h1>Controller selecting</h1>
            <table class="table table-bordered table-condensed monospace">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Type</th>
                        <th>Assembly</th>
                        <th>Version</th>
                        <th>Culture</th>
                        <th>Token</th>
                    </tr>
                </thead>
                <tbody data-bind="foreach: controllers">
                    <tr data-bind="css: { success: picked==true }">
                        <td data-bind="text: name"></td>
                        <td data-bind="text: typename"></td>
                        <td data-bind="text: assembly"></td>
                        <td data-bind="text: version"></td>
                        <td data-bind="text: culture"></td>
                        <td data-bind="text: token"></td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Actions -->
        <div class="row" data-bind="if: displayActions">
            <h1>Action selecting</h1>
            <table class="table table-bordered table-condensed monospace">
                <thead>
                    <tr>
                        <th rowspan="2">#</th>
                        <th colspan="3">Details</th>
                        <th colspan="2">By action name</th>
                        <th>By HTTP verb</th>
                        <th>Later stage</th>
                    </tr>
                    <tr>
                        <th>Verb</th>
                        <th>Name</th>
                        <th>Param</th>
                        <th>Action</th>
                        <th>Verb</th>
                        <th>Verb</th>
                        <th>Parameter</th>
                        <th>NonAction</th>
                    </tr>
                </thead>
                <tbody data-bind="foreach: action">
                    <tr data-bind="with: $data, css: { success: $data.foundWithSelectorsRun==true }">
                        <td data-bind="text: $index"></td>
                        <td data-bind="text: methods"></td>
                        <td data-bind="text: actionName"></td>
                        <td data-bind="text: param"></td>
                        <td data-bind="text: foundByActionName"></td>
                        <td data-bind="text: foundByActionNameWithRightVerb"></td>
                        <td data-bind="text: foundByVerb"></td>
                        <td data-bind="text: foundWithRightParam"></td>
                        <td data-bind="text: foundWithSelectorsRun"></td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
    @Scripts.Render("~/bundles/routedebugger")
</body>
</html>
