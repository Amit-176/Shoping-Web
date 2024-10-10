var dataTable;
$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    dataTable = $('#tbData').DataTable({
        "ajax": {
            "url": "/Admin/OrderManage/GetAll"
        },
        "columns": [
            { "data": "id", "width": "15%" },
            { "data": "name", "width": "15%" },
            { "data": "orderDate", "width": "25%" },
            { "data": "orderTotal", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                     <div class="text-center"> 
                     <a href="/Admin/OrderManage/ViewData/${data}" class="btn btn-info">
                       <i class="fas fa-info"></i>
                     </a>
                     </div>                     
                    `;
                }
            }
        ]
    })
}