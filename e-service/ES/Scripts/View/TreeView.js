var setting = {
    view: {
        selectedMulti: false
    },
    edit: {
        enable: true,
        showRemoveBtn: false,
        showRenameBtn: false
    },
    data: {
        keep: {
            parent: true,
            leaf: true
        },
        simpleData: {
            enable: true
        }
    }
};

$(document).ready(function () {
    $.fn.zTree.init($("#treeDemo"), setting, zNodes);
    $("#addLeaf").bind("click", { isParent: false }, add);
    $("#edit").bind("click", edit);
    $("#remove").bind("click", remove);

    var zTree = $.fn.zTree.getZTreeObj("treeDemo");
    var tid = "";
    if ($('#ActionModel').val() == "Delete") {
        tid = $('#ParentClassid').val();
    } else {
        tid = $('#Classid').val();
    }
    var tnode = zTree.getNodeByParam("ccd", tid);
    while (tnode != null) {
        tnode.open = true;
        tnode = tnode.getParentNode();
    }
    if (zTree.getNodeByParam("ccd", tid) != null) {
        var a = 0 + $('#scollBarPostion').val();
        $('#treediv').scrollTop(a);
    }
    zTree.refresh();
});

function add(e) {
    var zTree = $.fn.zTree.getZTreeObj("treeDemo"),
			isParent = e.data.isParent,
			nodes = zTree.getSelectedNodes(),
			treeNode = nodes[0];
    if (nodes.length == 0) {
        alert("請先選擇一個節點");
        return;
    }
    
    $('#edittable').show();
    $('#ActionModel').val("Add");
    $('#title').html("新增節點");
    $('#TargetTable').val(treeNode.id);
    $('#parentName').html(treeNode.name);
    $('#ParentClassid').val(treeNode.ccd);
    $('#Classid').val("");
    $('#Classname').val("");
    $('#Clevel').val(treeNode.clevel + 1);
};
function edit() {
    var zTree = $.fn.zTree.getZTreeObj("treeDemo"),
			nodes = zTree.getSelectedNodes(),
			treeNode = nodes[0];
    if (nodes.length == 0) {
        alert("請先選擇一個節點");
        return;
    }
    if (nodes.clevel == 0) {
        return;
    }
    $('#edittable').show();
    $('#ActionModel').val("Update");
    $('#TargetTable').val(treeNode.getParentNode().id);
    $('#title').html("編輯節點");
    $('#parentName').html(treeNode.getParentNode().name);
    $('#ParentClassid').val(treeNode.cpcd);
    $('#BeforeClassid').val(treeNode.ccd);
    $('#Classid').val(treeNode.ccd);
    $('#Classname').val(treeNode.cname);
    $('#Clevel').val(treeNode.clevel);
};

function remove(e) {
    var zTree = $.fn.zTree.getZTreeObj("treeDemo"),
			nodes = zTree.getSelectedNodes(),
			treeNode = nodes[0];
    if (nodes.length == 0) {
        alert("請先選擇一個節點!");
        return;
    }
    if (treeNode.children != null) {
        alert("尚有子節點不可刪除!");
        return;
    }
    $('#ActionModel').val("Delete");
    $('#TargetTable').val(treeNode.id);
    $('#Classid').val(treeNode.ccd);
    $('#Classname').val(treeNode.cname);
    $('#ParentClassid').val(treeNode.cpcd);
    if (confirm("確認刪除 " + treeNode.cname + " 嗎?")) {
        $('#ActionForm').submit();
    }

};
