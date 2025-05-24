Dropzone.options.dropzoneForm = {
    url: "https://localhost:5001/api/FileUpload/upload",
    paramName: "file",
    maxFilesize: 10, // MB
    acceptedFiles: ".csv,.xlsx,.xls,.json,.xml",
    autoProcessQueue: false,
    init: function () {
        var dz = this;

        // Trigger upload on button click
        document.querySelector("button[type='submit']").addEventListener("click", function (e) {
            e.preventDefault();
            dz.processQueue();
        });

        dz.on("success", function (file, response) {
            // Assuming response is JSON array of records
            displayDataTable(response.data);
        });

        dz.on("error", function (file, errorMessage) {
            alert("Upload failed: " + errorMessage);
        });
    }
};

$(document).ready(function () {
    $('.dataTables-example').DataTable({
        pageLength: 15,
        responsive: true,
        dom: 'Bfrtip',        
    });
});

// Display DataTable dynamically
function displayDataTable(data) {
    if (!Array.isArray(data) || data.length === 0) {
        alert("No data found.");
        return;
    }
     const columns = Object.keys(data[0].Fields);

    // Build table headers
    let headerRow = "<tr>";
    columns.forEach(col => {
        headerRow += `<th>${col}</th>`;
    });
    headerRow += "<th>Validation Errors</th>";
    headerRow += "</tr>";
    $(".dataTables-example thead").html(headerRow);

    // Build table rows
    let rowsHtml = "";
    data.forEach(item => {
        let row = "<tr>";
        columns.forEach(col => {
            row += `<td>${item.Fields[col]}</td>`;
        });
        var errorTd = ""
        item.ValidationErrors.forEach(err =>{
            errorTd += err + "<br>"
        });
        row += "<td class=\"red-text\">" + errorTd + "</td>";
        row += "</tr>";
        rowsHtml += row;
    });
    $(".dataTables-example tbody").html(rowsHtml);

    // Initialize DataTable
    $(".dataTables-example").DataTable();

    
}