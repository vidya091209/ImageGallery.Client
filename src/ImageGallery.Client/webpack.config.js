var isDevBuild = process.argv.indexOf('--env.prod') < 0;
var path = require('path');
var webpack = require('webpack');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var CleanWebpackPlugin = require('clean-webpack-plugin');

// Configuration in common to both client-side and server-side bundles
module.exports = {

    devtool: isDevBuild ? 'inline-source-map' : false,

    resolve: {
        extensions: ['.ts', '.js', '.json', '.css', '.scss', '.html'],
        alias: {
            jquery: path.resolve(__dirname, 'node_modules/jquery/dist/jquery.js')
        }
    },

    entry: {
        'polyfills': './ClientApp/app/polyfills.ts',
        'vendor': './clientApp/app/vendor.ts',
        'main-client': './ClientApp/boot-client.ts'
    },

    output: {
        filename: '[name].js',
        publicPath: '/dist/',
        path: path.join(__dirname, './wwwroot/dist')
    },

    module: {
        rules: [{
                test: /\.ts$/,
                exclude: [/\.spec\.ts$/, /node_modules/],
                use: ['ts-loader', 'angular2-router-loader', 'angular2-template-loader']
            }, {
                test: /\.html$/,
                loader: 'raw-loader'
            }, { // Load css files which are required in vendor.ts
                test: /\.css$/,
                loader: ExtractTextPlugin.extract({
                    fallback: 'style-loader',
                    use: {
                        loader: 'css-loader',
                        options: {
                            sourceMap: isDevBuild
                        }
                    }
                })
            }, {
                test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|eot|ttf)$/,
                use: [{
                    loader: 'url-loader',
                    options: { limit: 100000 }
                }]
            },
            { test: /jquery\.flot\.resize\.js$/, loader: 'imports-loader?this=>window' },
            { test: /\.scss$/, use: ['raw-loader', 'sass-loader'] },
            { test: /\.json$/, loader: 'json-loader' }
        ]
    },

    plugins: [
        new ExtractTextPlugin('[name].css'),
        new webpack.optimize.CommonsChunkPlugin({
            name: ['main-client', 'vendor', 'polyfills']
        }),
        // new CleanWebpackPlugin(['./wwwroot/dist/']),
        new webpack.ProvidePlugin({ $: 'jquery', jQuery: 'jquery', 'window.jQuery': 'jquery' })
    ].concat(isDevBuild ? [] : [
        // Plugins that apply in production builds only
        new webpack.optimize.UglifyJsPlugin({
            compress: {
                warnings: false
            },
            output: {
                comments: false
            },
            sourceMap: false
        })
    ])
};