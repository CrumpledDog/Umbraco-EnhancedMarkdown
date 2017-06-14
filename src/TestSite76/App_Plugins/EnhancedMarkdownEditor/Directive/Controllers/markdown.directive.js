function markDownDirective($rootScope, assetsService, dialogService, $timeout, contentResource, mediaResource) {
    // Usage <mark-down md="control.value.md"></mark-down>

    return {
        restrict: 'E',
        templateUrl: '/App_Plugins/EnhancedMarkdownEditor/Directive/Views/markdown.html',
        scope: {
            md: '='
        },
        link: function ($scope) {

            function Mark() {
                this.editor = new markDownObj("md");
                this.preview = "";
                this.uniqueId = ID();
            }

            function markDownObj(md) {
                this.alias = md;
            }

            function isHtml(str) {
                var is = /<[a-z][\s\S]*>/i.test(str);
                return is;
            }

            var ID = function () {
                // Math.random should be unique because of its seeding algorithm.
                // Convert it to base 36 (numbers + letters), and grab the first 9 characters
                // after the decimal.
                return Math.random().toString(36).substr(2, 9);
            };

            $scope.md = (!$.isEmptyObject($scope.md)) ? $scope.md : new Mark();

            // Generate NEW ID for each new MD regardless if it already exists
            $scope.md.uniqueId = ID();

            //console.log("Directive Scope", $scope.md);

            //handle old html from the RTE
            if (isHtml($scope.md)) {
                var tmp = $scope.md;

                $scope.md = new Mark();
                $scope.md.preview = tmp;
                $scope.md.editor.content = toMarkdown(tmp);
            }

            if (typeof $scope.md.editor === 'undefined') {
                var tmp = $scope.md;

                $scope.md = new Mark();

                $scope.md.preview = tmp.content;
                $scope.md.editor.content = tmp.content;
            }

            if ($scope.md.editor.content !== "") {
                $scope.preview = true;
                $scope.toggleStatus = "Edit";
            }
            else {
                $scope.preview = false;
                $scope.toggleStatus = "Preview";
            }

            $scope.toggle = function () {
                $scope.preview = !$scope.preview;

                $scope.toggleStatus = (!$scope.preview) ? "Preview" : "Edit";
            }


            assetsService
                    .load([
                        "lib/markdown/markdown.converter.js",
                        "lib/markdown/markdown.sanitizer.js",
                        "/app_plugins/EnhancedMarkdownEditor/lib/markdown/markdown.editor.js"
                    ])
                    .then(function () {


                        //this function will execute when all dependencies have loaded
                        // but in the case that they've been previously loaded, we can only 
                        // init the md editor after this digest because the DOM needs to be ready first
                        // so run the init on a timeout
                        $timeout(function () {
                            var converter2 = new Markdown.Converter();

                            //console.log('Converter2', converter2);

                            var editor2 = new Markdown.Editor(converter2, "-" + $scope.md.editor.alias + "-" + $scope.md.uniqueId, $scope.md.uniqueId, {});

                            //console.log('Editor2', $.extend({},editor2));

                            editor2.run();

                            //console.log('Editor2 Post Run', editor2);

                            var alias = $scope.md.uniqueId;
                            var markdowns = editor2.returnGlobalMarkdownCollection();

                            var extendMarkdowns = $.extend({}, markdowns);

                           // console.log('Mardkwon Glbal collection', extendMarkdowns)

                            var single = $.grep(extendMarkdowns.collection, function (e) { return e.id == alias; });

                            var singleExtend = $.extend({}, single);

                           // console.log('Single Exytend', singleExtend);


                            singleExtend[0].ref.hooks.set("previewHtml", function (text) {

                                //console.log('Text', text);

                                $scope.md.preview = text;
                                $scope.md.editor.content = text;
                            });

                            singleExtend[0].ref.hooks.set("postProcessing", function (updated) {
                                //console.log(updated);
                                $scope.md.preview = updated;
                                $scope.md.editor.content = updated;
                            });

                            //console.log('Current Hooks', singleExtend[0].ref.hooks.insertImageDialog.name);

                            if(singleExtend[0].ref.hooks.insertImageDialog.name === "returnFalse") {

                                //subscribe to the image dialog clicks
                                singleExtend[0].ref.hooks.set("insertImageDialog", function (callback) {

                                    //console.log('Image callback', callback);

                                    dialogService.mediaPicker({
                                        callback: function (data) {
                                            //console.log('data', data)

                                            callback(data.image, data.image.url, alias);

                                        }
                                    });

                                    return true; // tell the editor that we'll take care of getting the image url
                                });
                            }

                            singleExtend[0].ref.hooks.set("insertLinkDialog", function (callback) {


                                
                                dialogService.open({
                                    template: "/App_Plugins/EnhancedMarkdownEditor/Dialogs/Views/linkpickerwithanchor.html",
                                    callback: function (data) {
                                        //console.log('data', data);
                                        // check content type
                                        var localLink = "/umbLink:true";

                                        var anchor = (typeof data.anchor !== 'undefined') ? '/anchor:' + data.anchor : "";

                                        if (typeof data.target != 'undefined') {
                                            localLink += "/target:" + data.target;
                                        }
                                        if (typeof data.name != 'undefined') {
                                            localLink += "/title:" + data.name;
                                        }

                                        if (typeof data.id !== 'undefined') {
                                            localLink += "/localLink:" + data.id + anchor;
                                        } else if (typeof data.url === 'undefined') {
                                            // Anchor to self page
                                            localLink += anchor;
                                        } else {
                                            // External link
                                            localLink += "/extLink:" + data.url;
                                        }

                                        callback(data, localLink, alias);
                                    }
                                });
                                return true;
                            });
                        });
                    });

            //load the seperat css for the editor to avoid it blocking our js loading TEMP HACK
            assetsService.loadCss("lib/markdown/markdown.css");

            jQuery(document).ready(function ($) {
                $('body').on('focus', 'textarea.wmd-input', function () {
                    $(this).autosize();
                });
            });
        }
    };
};

angular.module('umbraco.directives').directive('markDown', markDownDirective);