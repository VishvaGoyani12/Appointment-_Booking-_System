var originalModal = $("#divModal").clone();

$("#divModal").on("hidden.bs.modal", function () {
    $("#divModal").remove(); 
    const myClone = originalModal.clone(); 
    $("body").append(myClone);
});

$(document).ready(function () {
    loadData();

    $('#btnAdd').click(function () {
        openPatientModal('/PatientAjax/Create');
    });

    $('#filterGender, #filterStatus').change(function () {
        $('#patientTable').DataTable().ajax.reload();
    });

    $('#filterJoinDate').on('input', function () {
        $('#patientTable').DataTable().ajax.reload();
    });
});

function openPatientModal(url) {
    $("#modalContent").load(url, function () {
        showModal();
    });
}

function showModal() {
    $("#divModal").modal('show');
}

$(document).on('submit', '#formCreateOrEdit', function (e) {
    e.preventDefault();
    var form = $(this);
    var token = form.find('input[name="__RequestVerificationToken"]').val();
    var patientId = form.find('input[name="Id"]').val();
    var isEdit = patientId && !isNaN(parseInt(patientId)) && parseInt(patientId) > 0;

    $.ajax({
        url: isEdit ? '/PatientAjax/Edit' : '/PatientAjax/Create',
        type: 'POST',
        data: form.serialize(),
        headers: { 'RequestVerificationToken': token },
        success: function (res) {
            if (res.success) {
                closeModal();
                loadData();
            } else {
                $('#modalContent').html(res); 
            }
        },
        error: function (xhr) {
            console.error("Save error:", xhr.responseText);
        }
    });
});

function closeModal() {
    $('#modalContent').html('');
    $('#divModal').modal('hide');
}

function editPatient(id) {
    openPatientModal(`/PatientAjax/Edit/${id}`);
}

function deletePatient(id) {
    if (confirm("Are you sure to delete?")) {
        $.post('/PatientAjax/Delete', { id: id }, function (res) {
            if (res.success) {
                loadData();
            }
        });
    }
}

function loadData() {
    $('#patientTable').DataTable({
        processing: true,
        serverSide: true,
        destroy: true,
        ajax: {
            url: '/PatientAjax/GetAll',
            type: 'POST',
            data: function (d) {
                d.gender = $('#filterGender').val();
                d.status = $('#filterStatus').val();
                d.joinDate = $('#filterJoinDate').val();
            }
        },
        columns: [
            { data: 'name', title: 'Name' },
            { data: 'gender', title: 'Gender' },
            { data: 'joinDate', title: 'Join Date' },
            { data: 'status', title: 'Status' },
            {
                data: 'id', title: 'Actions',
                render: function (data) {
                    return `
                        <button onclick="editPatient(${data})" class="btn btn-sm btn-warning">Edit</button>
                        <button onclick="deletePatient(${data})" class="btn btn-sm btn-danger">Delete</button>`;
                }
            }
        ]
    });
}
