
let dataTable;

$(document).ready(function () {
    LoadDataTable();
});

function LoadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            url: '/admin/user/getall',
            type: 'GET',
            datatype: 'json'
        },
        "columns": [
            { data: 'name', "width": "15%" },
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'company.name', "width": "15%" },
            { data: 'role', "width": "15%" },
            {
                data: { id: 'id', lockoutEnd: 'lockoutEnd' },
                "render": function (data) {
                    if (!data.lockoutEnd) {
                        return `<div class="btn-group w-75">
                                <a href="/admin/user/UserRole?userId=${data.id}" class="btn btn-primary rounded-2 mx-3">Permission</a>
                                <a onClick="Unlock('${data.id}')" class="btn btn-danger rounded-2">
                                <i class="bi bi-lock-fill"></i> Lock</a>
                                </div>`;
                    } else {
                        return `<div class="btn-group w-75">
                                <a href="/admin/product/UserRole?userId=${data.id}" class="btn btn-primary rounded-2 mx-3">Permission</a>
                                <a onClick="Unlock('${data.id}')" class="btn btn-success rounded-2">
                                <i class="bi bi-unlock-fill"></i> Unlock</a>
                                </div>`;
                    }
                },
                "width": "25%"
            }
        ]
    });
}

function Unlock(userId) {
    $.ajax({
        url: `/Admin/User/LockUnlock/${userId}`,
        type: 'POST',
        success: function (result) {
            dataTable.ajax.reload();
            toastr.success(result.message);
        },
        error: function (err) {
            toastr.error("Error: Could not unlock the user.");
        }
    });
}
