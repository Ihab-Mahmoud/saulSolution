
$(document).ready(function () {
    LoadDataTable();
});

function LoadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/admin/product/getall',
            type: 'GET',
            datatype: 'json'
        },
        "columns": [
            { data: 'title', "width": "15%" },
            { data: 'author', "width": "15%" },
            { data: 'isbn', "width": "15%" },
            { data: 'listPrice', "width": "15%" },
            { data: 'category.name', "width": "15%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="btn-group w-75">
                            <a href="/admin/product/upsert?id=${data}"  class="btn btn-primary rounded-2 mx-3">Edit</a>
                            <a onClick="DeleteProduct(${data})"  class="btn btn-danger rounded-2">Delete</a>
                        </div>`
                },
                "width": "15%"
            }
        ]
    });
}




function DeleteProduct(id) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url:`/admin/product/delete/${id}`,
                type: 'DELETE',
                success: function (result) {
                    // Reload the DataTable or update the UI accordingly
                    $('#tblData').DataTable().ajax.reload();
                    toastr.success(result.message);
                },
                error: function (err) {
                    alert("Error: Could not delete the product.");
                }
            });

        }
    });
}
