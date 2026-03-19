$(document).ready(function () {
    const areaOptions = ["BRAZING", "I-190", "OTHERS", "PLATING", "PLT3", "SPEXP", "SPEXP2"];
    let currentFilter = 'all';

    // Format relative time function (same as MonitorPR)
    function formatRelativeTime(data, type, row) {
        if (!data) return "";
        
        // For sorting or filtering, return the raw data just like the original date
        if (type === 'sort' || type === 'type') {
            return data;
        }
        
        var date = new Date(data);
        
        // If invalid date
        if (isNaN(date.getTime())) return data;

        if (type === 'filter') {
             return date.toLocaleString('en-GB');
        }

        // Current time
        var now = new Date();
        var diffMs = now - date;
        var diffMins = Math.floor(diffMs / 60000);
        var diffHours = Math.floor(diffMs / 3600000);
        
        var fullDateStr = date.toLocaleString('en-GB'); // dd/mm/yyyy, hh:mm:ss

        // Logic:
        // < 1 hour (60 mins) -> show X minutes ago
        // < 1 day (24 hours) -> show X hours ago
        // >= 1 day -> show full date
        
        if (diffMins < 60 && diffMins >= 0) {
            var timeStr = diffMins + " minutes ago";
            if (diffMins <= 1) timeStr = "Just now";
            else timeStr = diffMins + " minutes ago";

            return '<span title="' + fullDateStr + '" class="cursor-help border-b border-dotted border-gray-400">' + timeStr + '</span>';
        } else if (diffHours < 24 && diffHours >= 0) {
            var timeStr = diffHours + " hours ago";
            if (diffHours === 1) timeStr = "1 hour ago";
            
            return '<span title="' + fullDateStr + '" class="cursor-help border-b border-dotted border-gray-400">' + timeStr + '</span>';
        } else {
            return fullDateStr;
        }
    }

    const table = $('#tblConvertPO').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: basePath + '/ConvertPO/GetDataTable',
            type: 'POST',
            data: function (d) {
                // Add custom filter for convertpoflag
                if (currentFilter !== 'all') {
                    d.convertpoflag = currentFilter;
                }
            },
            error: function (xhr, error, code) {
                console.error('DataTable Error:', error);
                Swal.fire({
                    icon: 'error',
                    title: 'เกิดข้อผิดพลาด',
                    text: 'ไม่สามารถโหลดข้อมูลได้'
                });
            }
        },
        columns: [
            { 
                data: 'prno', 
                name: 'prno',
                autoWidth: true,
                orderable: true
            },
            {
                data: 'amount',
                name: 'amount',
                autoWidth: true,
                className: 'text-right',
                orderable: true,
                render: function (data) {
                    return data != null ? parseFloat(data).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '';
                }
            },
            {
                data: 'area',
                name: 'area',
                autoWidth: true,
                orderable: true,
                render: function (data, type, row) {
                    if (type === 'display') {
                        let options = '<select class="area-dropdown border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500" data-id="' + row.id + '">';
                        options += '<option value="">-- เลือก Area --</option>';
                        areaOptions.forEach(function (opt) {
                            const selected = data === opt ? 'selected' : '';
                            options += '<option value="' + opt + '" ' + selected + '>' + opt + '</option>';
                        });
                        options += '</select>';
                        return options;
                    }
                    return data;
                }
            },
            {
                data: 'credate',
                name: 'credate',
                autoWidth: true,
                orderable: true,
                render: function (data, type, row) {
                    return formatRelativeTime(data, type, row);
                }
            },
            { 
                data: 'creuser', 
                name: 'creuser',
                autoWidth: true,
                orderable: true
            },
            {
                data: 'updatedate',
                name: 'updatedate',
                autoWidth: true,
                orderable: true,
                render: function (data, type, row) {
                    return formatRelativeTime(data, type, row);
                }
            },
            { 
                data: 'updateuser', 
                name: 'updateuser',
                autoWidth: true,
                orderable: true
            },
            {
                data: 'convertpoflag',
                name: 'convertpoflag',
                autoWidth: true,
                orderable: true,
                render: function (data, type, row) {
                    if (type === 'display') {
                        const checked = data ? 'checked' : '';
                        const disabled = data ? 'disabled' : '';
                        return '<input type="checkbox" class="convertpo-checkbox w-4 h-4 text-primary-600 bg-gray-100 border-gray-300 rounded focus:ring-primary-500 cursor-pointer" data-id="' + row.id + '" data-prno="' + row.prno + '" ' + checked + ' ' + disabled + '>';
                    }
                    return data;
                }
            },
            {
                data: 'pono',
                name: 'pono',
                autoWidth: true,
                orderable: true,
                render: function (data, type, row) {
                    if (!row.convertpoflag) {
                        return '-';
                    }
                    return data || '-';
                }
            },
            {
                data: 'convertpodate',
                name: 'convertpodate',
                autoWidth: true,
                orderable: true,
                render: function (data, type, row) {
                    return formatRelativeTime(data, type, row);
                }
            },
            {
                data: 'printpoflag',
                name: 'printpoflag',
                autoWidth: true,
                orderable: true,
                render: function (data, type, row) {
                    if (type === 'display') {
                        const checked = data ? 'checked' : '';
                        return '<input type="checkbox" class="printpo-checkbox w-4 h-4 text-primary-600 bg-gray-100 border-gray-300 rounded focus:ring-primary-500 cursor-pointer" data-id="' + row.id + '" ' + checked + '>';
                    }
                    return data;
                }
            },
            {
                data: 'printpodate',
                name: 'printpodate',
                autoWidth: true,
                orderable: true,
                render: function (data, type, row) {
                    return formatRelativeTime(data, type, row);
                }
            }
        ],
        order: [[3, 'desc']], // Sort by credate descending
        pageLength: 25,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        language: {
            processing: "กำลังประมวลผล...",
            search: "ค้นหา:",
            lengthMenu: "แสดง _MENU_ รายการ",
            info: "แสดง _START_ ถึง _END_ จาก _TOTAL_ รายการ",
            infoEmpty: "แสดง 0 ถึง 0 จาก 0 รายการ",
            infoFiltered: "(กรองจากทั้งหมด _MAX_ รายการ)",
            loadingRecords: "กำลังโหลด...",
            zeroRecords: "ไม่พบข้อมูล",
            emptyTable: "ไม่มีข้อมูลในตาราง",
            paginate: {
                first: "หน้าแรก",
                previous: "ก่อนหน้า",
                next: "ถัดไป",
                last: "หน้าสุดท้าย"
            }
        }
    });

    // Handle area dropdown change
    $('#tblConvertPO tbody').on('change', '.area-dropdown', function () {
        const dropdown = $(this);
        const id = dropdown.data('id');
        const area = dropdown.val();

        // Validate that area is not empty or GENERAL
        if (!area || area.toUpperCase() === 'GENERAL') {
            Swal.fire({
                icon: 'warning',
                title: 'ไม่สามารถบันทึกได้',
                text: 'กรุณาเลือก Area ที่ไม่ใช่ GENERAL'
            });
            dropdown.val(dropdown.data('original-value') || '');
            return;
        }

        // Confirm before saving
        Swal.fire({
            title: 'ยืนยันการบันทึก',
            text: 'คุณต้องการบันทึกการเปลี่ยนแปลง Area หรือไม่?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#0284c7',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'บันทึก',
            cancelButtonText: 'ยกเลิก'
        }).then((result) => {
            if (result.isConfirmed) {
                // Save to database
                $.ajax({
                    url: basePath + '/ConvertPO/UpdateArea',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        id: id,
                        area: area
                    }),
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'สำเร็จ',
                                text: response.message,
                                timer: 1500,
                                showConfirmButton: false
                            });
                            dropdown.data('original-value', area);
                            table.ajax.reload(null, false);
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'ไม่สำเร็จ',
                                text: response.message
                            });
                            dropdown.val(dropdown.data('original-value') || '');
                        }
                    },
                    error: function (xhr) {
                        Swal.fire({
                            icon: 'error',
                            title: 'เกิดข้อผิดพลาด',
                            text: xhr.responseJSON?.message || 'ไม่สามารถบันทึกข้อมูลได้'
                        });
                        dropdown.val(dropdown.data('original-value') || '');
                    }
                });
            } else {
                dropdown.val(dropdown.data('original-value') || '');
            }
        });
    });

    // Store original value when dropdown is focused
    $('#tblConvertPO tbody').on('focus', '.area-dropdown', function () {
        $(this).data('original-value', $(this).val());
    });

    // Handle print PO checkbox change
    $('#tblConvertPO tbody').on('change', '.printpo-checkbox', function () {
        const checkbox = $(this);
        const id = checkbox.data('id');
        const printpoflag = checkbox.is(':checked');
        const originalState = !printpoflag;

        // Confirm before saving
        Swal.fire({
            title: 'ยืนยันการบันทึก',
            text: 'คุณต้องการบันทึกการเปลี่ยนแปลง Print PO Flag หรือไม่?',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#0284c7',
            cancelButtonColor: '#6b7280',
            confirmButtonText: 'บันทึก',
            cancelButtonText: 'ยกเลิก'
        }).then((result) => {
            if (result.isConfirmed) {
                // Save to database
                $.ajax({
                    url: basePath + '/ConvertPO/UpdatePrintPOFlag',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({
                        id: id,
                        printPOFlag: printpoflag
                    }),
                    success: function (response) {
                        if (response.success) {
                            Swal.fire({
                                icon: 'success',
                                title: 'สำเร็จ',
                                text: response.message,
                                timer: 1500,
                                showConfirmButton: false
                            });
                            table.ajax.reload(null, false);
                        } else {
                            Swal.fire({
                                icon: 'error',
                                title: 'ไม่สำเร็จ',
                                text: response.message
                            });
                            checkbox.prop('checked', originalState);
                        }
                    },
                    error: function (xhr) {
                        Swal.fire({
                            icon: 'error',
                            title: 'เกิดข้อผิดพลาด',
                            text: xhr.responseJSON?.message || 'ไม่สามารถบันทึกข้อมูลได้'
                        });
                        checkbox.prop('checked', originalState);
                    }
                });
            } else {
                checkbox.prop('checked', originalState);
            }
        });
    });

    // Handle convert PO checkbox change
    $('#tblConvertPO tbody').on('change', '.convertpo-checkbox', function () {
        const checkbox = $(this);
        const id = checkbox.data('id');
        const prno = checkbox.data('prno');
        const isChecked = checkbox.is(':checked');

        // Only allow checking, not unchecking
        if (!isChecked) {
            checkbox.prop('checked', true);
            return;
        }

        // Show loading
        Swal.fire({
            title: 'กำลังค้นหา PO No...',
            text: 'กรุณารอสักครู่',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        // Find PO No by PR No
        $.ajax({
            url: basePath + '/ConvertPO/FindPONoByPRNo',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                prno: prno
            }),
            success: function (response) {
                Swal.close();

                if (response.success) {
                    const pono = response.pono;
                    const found = response.found;

                    let confirmMessage = '';
                    let confirmIcon = 'question';

                    if (found) {
                        confirmMessage = 'พบหมายเลข PO: ' + pono + '\nคุณต้องการบันทึกหรือไม่?';
                        confirmIcon = 'success';
                    } else {
                        confirmMessage = 'ไม่พบหมายเลข PO\nคุณต้องการบันทึกหรือไม่?';
                        confirmIcon = 'warning';
                    }

                    // Confirm before saving
                    Swal.fire({
                        title: 'ยืนยันการบันทึก',
                        text: confirmMessage,
                        icon: confirmIcon,
                        showCancelButton: true,
                        confirmButtonColor: '#0284c7',
                        cancelButtonColor: '#6b7280',
                        confirmButtonText: 'บันทึก',
                        cancelButtonText: 'ยกเลิก'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            // Save to database
                            $.ajax({
                                url: basePath + '/ConvertPO/UpdateConvertPOFlag',
                                type: 'POST',
                                contentType: 'application/json',
                                data: JSON.stringify({
                                    id: id,
                                    pono: pono
                                }),
                                success: function (response) {
                                    if (response.success) {
                                        Swal.fire({
                                            icon: 'success',
                                            title: 'สำเร็จ',
                                            text: response.message,
                                            timer: 1500,
                                            showConfirmButton: false
                                        });
                                        table.ajax.reload(null, false);
                                    } else {
                                        Swal.fire({
                                            icon: 'error',
                                            title: 'ไม่สำเร็จ',
                                            text: response.message
                                        });
                                        checkbox.prop('checked', false);
                                    }
                                },
                                error: function (xhr) {
                                    Swal.fire({
                                        icon: 'error',
                                        title: 'เกิดข้อผิดพลาด',
                                        text: xhr.responseJSON?.message || 'ไม่สามารถบันทึกข้อมูลได้'
                                    });
                                    checkbox.prop('checked', false);
                                }
                            });
                        } else {
                            checkbox.prop('checked', false);
                        }
                    });
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'เกิดข้อผิดพลาด',
                        text: response.message || 'ไม่สามารถค้นหา PO No ได้'
                    });
                    checkbox.prop('checked', false);
                }
            },
            error: function (xhr) {
                Swal.close();
                Swal.fire({
                    icon: 'error',
                    title: 'เกิดข้อผิดพลาด',
                    text: xhr.responseJSON?.message || 'ไม่สามารถค้นหา PO No ได้'
                });
                checkbox.prop('checked', false);
            }
        });
    });

    // Filter button click handler
    $('.filter-btn').on('click', function () {
        const filter = $(this).data('filter');
        currentFilter = filter;

        // Update button styles
        $('.filter-btn').removeClass('bg-primary-600 text-white shadow-sm hover:bg-primary-700')
            .addClass('bg-white text-gray-700 border border-gray-300 hover:bg-gray-50');
        $(this).removeClass('bg-white text-gray-700 border border-gray-300 hover:bg-gray-50')
            .addClass('bg-primary-600 text-white shadow-sm hover:bg-primary-700');

        // Reload table with filter
        table.ajax.reload();
    });

    // Update counts function
    function updateCounts() {
        $.ajax({
            url: basePath + '/ConvertPO/GetCounts',
            type: 'GET',
            success: function (response) {
                $('#count-all').text(response.totalCount);
                $('#count-converted').text(response.convertedCount);
                $('#count-not-converted').text(response.notConvertedCount);
            },
            error: function (xhr) {
                console.error('Failed to update counts:', xhr);
            }
        });
    }

    // Override table reload to update counts
    const originalReload = table.ajax.reload;
    table.ajax.reload = function (callback, resetPaging) {
        originalReload.call(table, function () {
            updateCounts();
            if (callback) callback();
        }, resetPaging);
    };
});
