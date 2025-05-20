var originalModal = $("#divModal").clone();

$("#divModal").on("hidden.bs.modal", function () {
    $("#divModal").remove(); 
    const myClone = originalModal.clone(); 
    $("body").append(myClone);
});

$(document).ready(function () {
    loadData();

    $('#btnAdd').click(function () {
        openAppointmentModal('/AppointmentAjax/Create');
    });

    $('#filterPatient, #filterDoctor, #filterStatus').change(function () {
        $('#appointmentTable').DataTable().ajax.reload();
    });
});

function openAppointmentModal(url) {
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
    var appointmentId = form.find('input[name="Id"]').val();
    var isEdit = appointmentId && !isNaN(parseInt(appointmentId)) && parseInt(appointmentId) > 0;

    $.ajax({
        url: isEdit ? '/AppointmentAjax/Edit' : '/AppointmentAjax/Create',
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
            console.error("Error during save:", xhr.responseText);
        }
    });
});

function loadData() {
    $('#appointmentTable').DataTable({
        processing: true,
        serverSide: true,
        destroy: true,
        ajax: {
            url: '/AppointmentAjax/GetAll',
            type: 'POST',
            data: function (d) {
                d.patientId = $('#filterPatient').val();
                d.doctorId = $('#filterDoctor').val();
                d.status = $('#filterStatus').val();
            }
        },
        columns: [
            { data: 'patientName', title: 'Patient' },
            { data: 'doctorName', title: 'Doctor' },
            { data: 'appointmentDate', title: 'Date' },
            { data: 'description', title: 'Description' },
            { data: 'status', title: 'Status' },
            {
                data: 'id', title: 'Actions',
                render: function (data) {
                    return `
                        <button onclick="editAppointment(${data})" class="btn btn-sm btn-warning">Edit</button>
                        <button onclick="deleteAppointment(${data})" class="btn btn-sm btn-danger">Delete</button>`;
                }
            }
        ]
    });
}

function editAppointment(id) {
    openAppointmentModal(`/AppointmentAjax/Edit/${id}`);
}

function deleteAppointment(id) {
    if (confirm("Are you sure to delete this appointment?")) {
        $.post('/AppointmentAjax/Delete', { id: id }, function (res) {
            if (res.success) {
                loadData();
            }
        });
    }
}
