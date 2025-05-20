var originalModal = $("#divModal").clone();

$("#divModal").on("hidden.bs.modal", function () {
    $("#divModal").remove(); 
    const myClone = originalModal.clone(); 
    $("body").append(myClone); 
});

$(document).ready(function () {
    loadSpecialistFilter();
    loadData();

    $('#btnAdd').click(function () {
        openDoctorModal('/DoctorAjax/Create');
    });

    $('#filterGender, #filterStatus, #filterSpecialistIn').change(function () {
        $('#doctorTable').DataTable().ajax.reload();
    });
});

function openDoctorModal(url) {
    $("#modalContent").load(url, function () {
        showModal();
    });
}

function showModal() {
    $("#divModal").modal('show');
}

function closeModal() {
    $('#modalContent').html('');
    $('#divModal').modal('hide');
}

$(document).on('submit', '#formCreateOrEdit', function (e) {
    e.preventDefault();

    var form = $(this);
    var token = form.find('input[name="__RequestVerificationToken"]').val();
    var doctorId = form.find('input[name="Id"]').val();
    var isEdit = doctorId && !isNaN(parseInt(doctorId)) && parseInt(doctorId) > 0;

    $.ajax({
        url: isEdit ? '/DoctorAjax/Edit' : '/DoctorAjax/Create',
        type: 'POST',
        data: form.serialize(),
        headers: {
            'RequestVerificationToken': token
        },
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

function loadSpecialistFilter() {
    $.getJSON('/DoctorAjax/GetSpecialistList', function (data) {
        var select = $('#filterSpecialistIn');
        $.each(data, function (i, item) {
            select.append($('<option>', {
                value: item,
                text: item
            }));
        });
    });
}

function editDoctor(id) {
    openDoctorModal(`/DoctorAjax/Edit/${id}`);
}

function deleteDoctor(id) {
    if (confirm("Are you sure to delete?")) {
        $.post('/DoctorAjax/Delete', { id: id }, function (res) {
            if (res.success) {
                loadData();
            }
        });
    }
}

function loadData() {
    $('#doctorTable').DataTable({
        processing: true,
        serverSide: true,
        destroy: true,
        ajax: {
            url: '/DoctorAjax/GetAll',
            type: 'POST',
            data: function (d) {
                d.gender = $('#filterGender').val();
                d.status = $('#filterStatus').val();
                d.specialistIn = $('#filterSpecialistIn').val();
            }
        },
        columns: [
            { data: 'name', title: 'Name' },
            { data: 'gender', title: 'Gender' },
            { data: 'specialistIn', title: 'Specialist In' },
            { data: 'status', title: 'Status' },
            {
                data: 'id', title: 'Actions',
                render: function (data) {
                    return `
                        <button onclick="editDoctor(${data})" class="btn btn-sm btn-warning">Edit</button>
                        <button onclick="deleteDoctor(${data})" class="btn btn-sm btn-danger">Delete</button>`;
                }
            }
        ]
    });
}
