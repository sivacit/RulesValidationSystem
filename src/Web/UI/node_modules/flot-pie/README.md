# flot-pie

_Created by Brian Medendorp
Updated with contributions from btburnett3, Anthony Aragues and Xavi Ivars_

_Note: I am not the developer or maintainer, I am setting it up for use on NPM._

Flot plugin for rendering pie charts.

**Copyright (c) 2007-2014 IOLA and Ole Laursen.**
**Licensed under the MIT license.**

The plugin assumes that each series has a single data value, and that each
value is a positive integer or zero.  Negative numbers don't make sense for a
pie chart, and have unpredictable results.  The values do NOT need to be
passed in as percentages; the plugin will calculate the total and per-slice
percentages internally.

The plugin supports these options:

```
series: {
    pie: {
        show: true/false
        radius: 0-1 for percentage of fullsize, or a specified pixel length, or 'auto'
        innerRadius: 0-1 for percentage of fullsize or a specified pixel length, for creating a donut effect
        startAngle: 0-2 factor of PI used for starting angle (in radians) i.e 3/2 starts at the top, 0 and 2 have the same result
        tilt: 0-1 for percentage to tilt the pie, where 1 is no tilt, and 0 is completely flat (nothing will show)
        offset: {
            top: integer value to move the pie up or down
            left: integer value to move the pie left or right, or 'auto'
        },
        stroke: {
            color: any hexidecimal color value (other formats may or may not work, so best to stick with something like '#FFF')
            width: integer pixel width of the stroke
        },
        label: {
            show: true/false, or 'auto'
            formatter:  a user-defined function that modifies the text/style of the label text
            radius: 0-1 for percentage of fullsize, or a specified pixel length
            background: {
                color: any hexidecimal color value (other formats may or may not work, so best to stick with something like '#000')
                opacity: 0-1
            },
            threshold: 0-1 for the percentage value at which to hide labels (if they're too small)
        },
        combine: {
            threshold: 0-1 for the percentage value at which to combine slices (if they're too small)
            color: any hexidecimal color value (other formats may or may not work, so best to stick with something like '#CCC'), if null, the plugin will automatically use the color of the first slice to be combined
            label: any text value of what the combined slice should be labeled
        }
        highlight: {
            opacity: 0-1
        }
    }
}
```