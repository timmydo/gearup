// This file in the main entry point for defining grunt tasks and using grunt plugins.
// Click here to learn more. http://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409

module.exports = function (grunt) {
	grunt.initConfig({
		typescript: {
			debug: {
				src: ['app/ts/**/*.ts'],
				dest: 'wwwroot/app/app.js',
				options: {
					references: ['typings/tsd.d.ts'],
					module: 'es6', //or commonjs 
					target: 'es5', //or es3 
					rootDir: 'app/ts',
					sourceMap: false,
					declaration: true
				}
			}
		},
		tsd: {
			refresh: {
				options: {
					// execute a command
					command: 'reinstall',

					//optional: always get from HEAD
					latest: true,

					// specify config file
					config: 'tsd.json',

					// experimental: options to pass to tsd.API
					opts: {
						// props from tsd.Options
					}
				}
			}
		},
		cssmin: {
			vendorcss: {
				files: {
					'wwwroot/css/bootstrap.css': ['bower_components/bootstrap/dist/css/bootstrap.min.css'],
					'wwwroot/css/font-awesome.css': ['bower_components/Font-Awesome/css/font-awesome.min.css'],
					'wwwroot/css/bootstrap-slider.css': ['bower_components/seiyria-bootstrap-slider/dist/css/bootstrap-slider.css']
				}

			},
			sitecss: {
				options: {
					banner: '/* My minified css file */'
				},
				files: {
					'wwwroot/css/site.min.css': [
                        'app/css/site.css']
				}
			}
		},
		clean: {
			files: { src: ['appjs', 'wwwroot/app', 'wwwroot/lib', 'wwwroot/css'] }
		},
		copy: {
			img: {
				cwd: 'app/img',
				src: '*.gif',
				dest: 'wwwroot/img/',
				expand: true
			},
			fonts: {
				expand: true,
				cwd: 'bower_components/font-awesome',
				src: ['fonts/*.*'],
				dest: 'wwwroot/',
			}
		},
		concat: {
			devjs: {
				nonull: true,
				src: ['bower_components/es5-shim/es5-shim.js',
					'bower_components/jquery/dist/jquery.js',
                    'bower_components/jquery-validation/jquery.validate.js',
                    'bower_components/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js',
                    'bower_components/bootstrap/dist/js/bootstrap.js',
                    'bower_components/seiyria-bootstrap-slider/dist/bootstrap-slider.min.js',
					"bower_components/typeahead.js/dist/typeahead.jquery.js",
					'bower_components/holderjs/holder.js',
					'bower_components/moment/moment.js',
                    'bower_components/handlebars/handlebars.js',
                    'bower_components/ember/ember.debug.js'],
				dest: 'wwwroot/lib/vendor-dev.js'
			},
			applib: {
				nonull: true,
				src: ['bower_components/es5-shim/es5-shim.min.js',
                    'bower_components/jquery/dist/jquery.min.js',
                    'bower_components/jquery-validation/jquery.validate.js',
                    'bower_components/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js',
                    'bower_components/bootstrap/dist/js/bootstrap.min.js',
                    'bower_components/seiyria-bootstrap-slider/dist/bootstrap-slider.min.js',
					"bower_components/typeahead.js/dist/typeahead.jquery.min.js",
					'bower_components/holderjs/holder.min.js',
					'bower_components/moment/moment.js',
                    'bower_components/handlebars/handlebars.min.js',
                    'bower_components/ember/ember.min.js'
				],
				dest: 'wwwroot/lib/vendor.js'
			}

		},
		/*
		uglify: {
			options: {
			},
			
		},
		*/
		
		/*
		bower: {
			install: {
				options: {
					targetDir: "wwwroot/lib",
					layout: "byComponent",
					cleanTargetDir: false
				}
			}
		},
		*/
		tslint: { // FIXME
			options: {
				//configuration: grunt.file.readJSON("tslint.json")
			},
			files: {
				src: ['app/ts/**.ts']
			}
		},
		emberTemplates: {
			compile: {
				options: {
					templateFileExtensions: /\.html/,
					//amd: true,
					templateBasePath: 'app/html',
					templateCompilerPath: 'bower_components/ember/ember-template-compiler.js',
					handlebarsPath: 'bower_components/handlebars/handlebars.js',
				    templateNamespace: 'HTMLBars'					
				},
				files: {
					"wwwroot/app/templates.js": ["app/html/**/*.html"]
				}
			}
		},
		watch: {
			ts: {
				files: 'app/ts/*.ts',
				tasks: ['typescript'],
				options: {
					debounceDelay: 250,
				},
			},
			html: {
				files: 'app/html/**/*.html',
				tasks: ['emberTemplates'],
				options: {
					debounceDelay: 250,
				},
			},
			css: {
				files: 'app/css/**.css',
				tasks: ['cssmin'],
				options: {
					debounceDelay: 250,
				},
			}
		},

	});

	// This command registers the default task which will install bower packages into wwwroot/lib
	grunt.registerTask("default", ["clean", "typescript", "copy", "concat", "cssmin", "emberTemplates"]);

	// The following line loads the grunt plugins.
	// This line needs to be at the end of this this file.
	grunt.loadNpmTasks("grunt-bower-task");
	grunt.loadNpmTasks('grunt-typescript');
	grunt.loadNpmTasks('grunt-contrib-cssmin');
	grunt.loadNpmTasks('grunt-contrib-uglify');
	grunt.loadNpmTasks('grunt-contrib-copy');
	grunt.loadNpmTasks('grunt-contrib-clean');
	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-concat');
	grunt.loadNpmTasks('grunt-tslint');
	grunt.loadNpmTasks('grunt-ember-templates');

};