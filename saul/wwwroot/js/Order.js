$(document).ready(function () {
    var url = window.location.search;

    if (url.includes("pending")) {
        LoadDataTable("pending");
    } else {
        if (url.includes("completed")) {
            LoadDataTable("completed");

        } else {
            if (url.includes("inprocess")) {
                LoadDataTable("inprocess");

            } else {
                if (url.includes("approved")) {
                    LoadDataTable("approved");

                } else {
                    LoadDataTable("all");
                }
            }
        }
    }
});

function LoadDataTable(orderStatus) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/admin/order/getall?orderStatus=' + orderStatus,
            type: 'GET',
            datatype: 'json'
        },
        "columns": [
            { data: 'id', "width": "15%" },
            { data: 'name', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'applicationUser.email', "width": "15%" },
            { data: 'orderStatus', "width": "15%" },
            { data: 'orderTotal', "width": "15%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="btn-group w-75">
                            <a href="/admin/order/details?OrderId=${data}"  class="btn btn-primary rounded-2 mx-3">Edit</a>
                        </div>`
                },
                "width": "15%"
            }
        ]
    });
}




