﻿<template>
  <div>
    <!-- 面包屑导航区域 -->
    <el-breadcrumb separator-class="el-icon-arrow-right">
      <el-breadcrumb-item :to="{ path: '/home' }">首页</el-breadcrumb-item>
      <el-breadcrumb-item>权限管理</el-breadcrumb-item>
      <el-breadcrumb-item>权限列表</el-breadcrumb-item>
    </el-breadcrumb>
    <!-- 卡片试图区域 -->
    <el-card>
      <!-- 搜索与添加区域 -->

      <el-row :gutter="20">
        <el-col :span="7">
          <el-input
            placeholder="请输入搜索的权限名称"
            class="input-with-select"
            v-model="queryInfo.query"
            clearable
            @clear="getSysPermissionList"
          >
            <el-button
              slot="append"
              icon="el-icon-search"
              @click="getSysPermissionList"
            ></el-button>
          </el-input>
        </el-col>
        <el-col :span="4">
          <el-button type="primary" @click="showAddOrEditDialog()" v-if="$hasPermission(['permissionManager.createPermission'])"
            >添加权限</el-button
          >
        </el-col>
      </el-row>
      <!-- 权限列表 -->
      <tree-table
        class="tree-table"
        :selection-type="false"
        :expand-type="false"
        :data="sysPermissionList"
        :columns="columns"
        :show-index="true"
        border
        stripe
        show-row-hover
        tree-type
       :is-fold="true"
      >
        <template slot="typeName" slot-scope="scope">
          <el-tag v-if="scope.row.typeName === '分组'">
            {{ scope.row.typeName }}
          </el-tag>
          <el-tag type="success" v-else-if="scope.row.typeName === '菜单'">
            {{ scope.row.typeName }}
          </el-tag>
          <el-tag type="info" v-else-if="scope.row.typeName === '操作点'">
            {{ scope.row.typeName }}
          </el-tag>
        </template>
        <template slot="opt" slot-scope="scope">
          <!-- 修改按钮 -->
          <el-button
            type="primary"
            icon="el-icon-edit"
            size="mini"
            @click="showAddOrEditDialog(scope.row.id)" v-if="$hasPermission(['permissionManager.editPermission'])"
            >编辑
          </el-button>
          <!-- 删除按钮 -->
          <el-button
            type="danger"
            icon="el-icon-delete"
            size="mini"
            @click="removeSysPermissionById(scope.row.id)" v-if="$hasPermission(['permissionManager.deletePermission'])"
            >删除
          </el-button>
        </template>
      </tree-table>
    </el-card>

    <!-- 添加权限的对话框 -->
    <el-dialog
      :title="(addOrEditForm.id > 0 ? '编辑' : '新增') + '权限'"
      :visible.sync="addOrEditDialogVisible"
      width="60%"
      @close="addOrEditDialogClosed"
    >
      <!-- 内容主体区域 -->

      <el-tabs v-model="activeName">
        <el-tab-pane label="权限管理" name="first">
          <el-form
            :model="addOrEditForm"
            :rules="addOrEditFormRules"
            ref="addOrEditFormRef"
            label-width="120px"
          >
            <el-input
              v-model="addOrEditForm.id"
              prop="id"
              type="hidden"
            ></el-input>

            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item prop="label"  label="名称">
                  <el-input v-model="addOrEditForm.label"></el-input>
                </el-form-item>
              </el-col>

              <el-col :span="12">
                <el-form-item label="父级节点：" prop="selectedKeys">
                  <el-cascader
                    :key="cascaderKey"
                    v-model="selectedKeys"
                    :options="groupTree"
                    style="width: 100%"
                    placeholder="试试搜索关键词"
                    :props="{
                      expandTrigger: 'hover',
                      value: 'id',
                      label: 'label',
                      children: 'children',
                    }"
                    @change="parentKeyChange"
                    filterable
                    clearable
                    change-on-select="true"
                  ></el-cascader>
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="权限类型：" prop="type">
                  <el-select
                    v-model="addOrEditForm.type"
                    placeholder="请选择"
                    style="width: 100%"
                  >
                    <el-option
                      v-for="item in permissionTypeOptions"
                      :key="item.value"
                      :label="item.label"
                      :value="item.value"
                    >
                    </el-option>
                  </el-select>
                </el-form-item>
              </el-col>

              <el-col :span="12">
                <el-form-item label="权限编码：" prop="code">
                  <el-input v-model="addOrEditForm.code"></el-input>
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label="接口：" prop="apiId">
                  <el-input v-model="addOrEditForm.api" :disabled="addOrEditForm.type!=3"></el-input>
                </el-form-item>
              </el-col>

              <el-col :span="12">
                <el-form-item label="视图：" prop="viewId">
                  <el-input v-model="addOrEditForm.view" :disabled="addOrEditForm.type!=2"></el-input>
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label=" 图标：" prop="icon">
                  <el-input v-model="addOrEditForm.icon" :disabled="addOrEditForm.type!=2 && addOrEditForm.type!=1"></el-input>
                </el-form-item>
              </el-col>

              <el-col :span="12">
                <el-form-item label="菜单访问地址：" prop="path">
                  <el-input v-model="addOrEditForm.path" :disabled="addOrEditForm.type!=2"></el-input>
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label=" 可关闭：" prop="closable">
                  <el-switch
                    v-model="addOrEditForm.closable"
                    active-text="是"
                    inactive-text="否"
                  >
                  </el-switch>
                </el-form-item>
              </el-col>

              <el-col :span="12">
                <el-form-item label=" 是否隐藏：" prop="hidden">
                  <el-switch
                    v-model="addOrEditForm.hidden"
                    active-text="是"
                    inactive-text="否"
                  >
                  </el-switch>
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label=" 打开新窗口：" prop="newWindow">
                  <el-switch
                    v-model="addOrEditForm.newWindow"
                    active-text="是"
                    inactive-text="否"
                  >
                  </el-switch>
                </el-form-item>
              </el-col>

              <el-col :span="12">
                <el-form-item label=" 打开组：" prop="opened">
                  <el-switch
                    v-model="addOrEditForm.opened"
                    active-text="是"
                    inactive-text="否"
                  >
                  </el-switch>
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label=" 排序：" prop="sort">
                  <el-input v-model="addOrEditForm.sort"></el-input>
                </el-form-item>
              </el-col>

              <el-col :span="12">
                <el-form-item label=" 链接外显：" prop="external">
                  <el-switch
                    v-model="addOrEditForm.external"
                    active-text="是"
                    inactive-text="否"
                  >
                  </el-switch>
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="20">
              <el-col :span="12">
                <el-form-item label=" 描述：" prop="description">
                  <el-input v-model="addOrEditForm.description"></el-input>
                </el-form-item>
              </el-col>
            </el-row>
          </el-form>
        </el-tab-pane>
      </el-tabs>

      <!-- 底部区域 -->
      <span slot="footer" class="dialog-footer">
        <el-button @click="addOrEditDialogVisible = false">取 消</el-button>
        <el-button type="primary" @click="addOrEditSysPermission()"
          >确 定</el-button
        >
      </span>
    </el-dialog>
  </div>
</template>

<script>
  import {
    listToTree,
    getTreeParents,
    getListParents,
    getTreeParentsWithSelf,
  } from '../../utils/tree.js'
  
  export default {
    data() {

      return {
        queryInfo: {
          query: '',
        },

        sysPermissionList: [],
        activeName: 'first',
        // 控制添加权限对话框的显示与隐藏
        addOrEditDialogVisible: false,
        // 添加权限的表单数据
        addOrEditForm: {
          id: '',
          parentId: 0,
          label: '',
          code: '',
          type: '',
          view: '',
          api: '',
          path: '',
          icon: '',
          hidden: false,
          closable: true,
          opened: true,
          newWindow: true,
          external: true,
          sort: 0,
          description: '',


        },
        permissionTypeOptions: [{
          value: 1,
          label: '分组'
        }, {
          value: 2,
          label: '菜单'
        }, {
          value: 3,
          label: '操作点'
        }],
        groupTree: [], //编辑权限 父选框
        selectedKeys: [0],
        cascaderKey: 1,
        editState: false, //编辑状态

        // 添加表单的验证规则对象
        addOrEditFormRules: {
          parentId: [{
            required: true,
            message: '请输入父级节点',
            trigger: 'blur'
          }, ],
         
          label: [{
              required: true,
              message: '请输入权限名称',
              trigger: 'blur'
            },
            {
              max: 50,
              message: '权限名称的长度在50字符之内',
              trigger: 'blur'
            },
          ],
          code: [{
              required: true,
              message: '请输入权限编码',
              trigger: 'blur'
            },
            {
              max: 550,
              message: '权限编码的长度在550字符之内',
              trigger: 'blur'
            },
          ],
          type: [{
            required: true,
            message: '请输入权限类型',
            trigger: 'blur'
          }, ],
          view: [{
            required: false,
            message: '请输入视图',
            trigger: 'blur'
          }, ],
          api: [{
            required: false,
            message: '请输入接口',
            trigger: 'blur'
          }, ],
          path: [{
              required: false,
              message: '请输入菜单访问地址',
              trigger: 'blur'
            },
            {
              max: 500,
              message: '菜单访问地址的长度在500字符之内',
              trigger: 'blur'
            },
          ],
          icon: [{
              required: false,
              message: '请输入 图标',
              trigger: 'blur'
            },
            {
              max: 100,
              message: ' 图标的长度在100字符之内',
              trigger: 'blur'
            },
          ],
          hidden: [{
            required: false,
            message: '请选择是否隐藏',
            trigger: 'blur'
          }, ],
          closable: [{
            required: false,
            message: '请输入 可关闭',
            trigger: 'blur'
          }, ],
          opened: [{
            required: false,
            message: '请输入 打开组',
            trigger: 'blur'
          }, ],
          newWindow: [{
            required: false,
            message: '请输入 打开新窗口',
            trigger: 'blur'
          }, ],
          external: [{
            required: false,
            message: '请输入 链接外显',
            trigger: 'blur'
          }, ],
          sort: [{
            required: false,
            message: '请输入 排序',
            trigger: 'blur'
          }, ],
          description: [{
              required: false,
              message: '请输入 描述',
              trigger: 'blur'
            },
            {
              max: 100,
              message: ' 描述的长度在100字符之内',
              trigger: 'blur'
            },
          ],

        },
        // 控制修改权限对话框的显示与隐藏
        editDialogVisible: false,

        columns: [
          {//第一列默认是树形符号打开的操作，id列如果放这里会有变形
            label: '权限名称',
            minWidth: '260px',
            prop: 'label'
          },
           {
            label: 'Id',
            prop: 'id'
          },
          {
            label: '权限类型',
            prop: 'typeName',
            type: 'template',
            template: 'typeName',
          },
           {
            label: '图标',
             width: '130px',
            prop: 'icon'
          },
          {
            label: '权限编码',
             width: '130px',
            prop: 'code'
          },
          {
            label: '接口',
            width: '130px',
            prop: 'api'
          },
          {
            label: '菜单访问地址',
            width: '130px',
            prop: 'path'
          },
         /*  {
            label: '图标',
            prop: 'icon',
            minWidth: '160px',
          },
          {
            label: '描述',
            width: '200px',
            prop: 'description'
          }, */
          {
            label: '操作',
            minWidth: '200px',
            type: 'template',
            template: 'opt',
          },
        ]
      }
    },
    created() {
      this.getSysPermissionList()
     //权限控制
        const isAllowed = this.$hasPermission(['permissionManager.deletePermission']) || this.$hasPermission([
        'permissionManager.editPermission'
      ]);
      if (!isAllowed) {//过滤权限
        this.columns = _.cloneDeep(this.columns).filter(x => x.label !== '操作')
      }
    },
    methods: {
      async getSysPermissionList() {

        var data = await this.$postRequest(this.$sysPermissionManagerUrl, {
          queryString: this.queryInfo.query,
        })
        this.sysPermissionList = listToTree(data)

      },
     
    
      //获取指定的权限标识
      getPermissionType(typeName) {
        if (typeName === '分组') {
          return 'primary'
        }

        if (typeName === '菜单') {
          return 'warning'
        }
        if (typeName === '接口') {
          return 'info'
        }
        return 'info'
      },
      // 监听添加权限对话框的关闭事件
      addOrEditDialogClosed() {
        this.editState = false //编辑状态改为false
        this.addOrEditForm = {
           id: '',
          parentId: 0,
          label: '',
          code: '',
          type: '',
          view: '',
          api: '',
          path: '',
          icon: '',
          hidden: false,
          closable: true,
          opened: true,
          newWindow: true,
          external: true,
          sort: 0,
          description: '',

          },
           ++this.cascaderKey//处理一些vue异常报错需要用到的key，需要加载改变地方，让它的key变化
          this.groupTree = [],
          this.selectedKeys = [], //重置父级分类选中内容
          this.$refs.addOrEditFormRef.resetFields()
      },
      // 点击按钮，添加权限
      addOrEditSysPermission() {
        this.$refs.addOrEditFormRef.validate(async (valid) => {
          if (!valid) return

          if (this.addOrEditForm.id > 0) {

            //编辑状态
            // 发起修改权限信息的数据请求
            const {
              data: res
            } = await this.$http.put(
              this.$sysPermissionManager_EditSysPermissionUrl,
              this.addOrEditForm
            )

            if (res.code !== 200) {
              return this.$message.error('更新权限信息失败！'+res.message)
            }

            // 提示修改成功
            this.$message.success('更新权限信息成功！')
          } else {
            // 可以发起添加权限的网络请求
            const {
              data: res
            } = await this.$http.post(
              this.$sysPermissionManager_CreateSysPermissionUrl,
              this.addOrEditForm
            )

            if (res.code !== 200) {
              this.$message.error('添加权限失败！'+res.message)
            }

            this.$message.success('添加权限成功！')
          }

          // 隐藏添加权限的对话框
          this.addOrEditDialogVisible = false
          // 重新获取权限列表数据
          this.getSysPermissionList()
        })
      },
      // 根据Id删除对应的权限信息
      async removeSysPermissionById(id) {
        // 弹框询问权限是否删除数据
        const confirmResult = await this.$confirm(
          '此操作将永久删除该权限, 是否继续?',
          '提示', {
            confirmButtonText: '确定',
            cancelButtonText: '取消',
            type: 'warning',
          }
        ).catch((err) => err)

        // 如果权限确认删除，则返回值为字符串 confirm
        // 如果权限取消了删除，则返回值为字符串 cancel
        // console.log(confirmResult)
        if (confirmResult !== 'confirm') {
          return this.$message.info('已取消删除')
        }

        const {
          data: res
        } = await this.$http.delete(
          this.$sysPermissionManager_DeleteSysPermissionUrl + '?id=' + id
        )

        if (res.code !== 200) {
          return this.$message.error('删除权限失败！'+res.message)
        }

        this.$message.success('删除权限成功！')
        this.getSysPermissionList()
      },

    
      // 展示权限的对话框
      async showAddOrEditDialog(id) {
        this.addOrEditDialogVisible = true
         
        if (id > 0) {
          //编辑状态
          // console.log(id)
          const {
            data: res
          } = await this.$http.get(
            this.$sysPermissionManager_GetSysPermissionUrl + '?id=' + id
          )

          if (res.code !== 200) {
            return this.$message.error('查询权限信息失败！'+res.message)
          }


          this.addOrEditForm = res.data
            const tempData = await this.$postRequest(this.$sysPermissionManager_GetSysPermissionFilterByPidUrl,"pid="+id)
           // 分组树
        const groups = tempData
        //要过滤自身的，避免自己选自己，对应的子节点都已经在后台过滤掉了
        this.groupTree = listToTree(_.cloneDeep(groups.filter(x=>x.id!=id)), {
          id: 0,
          parentId: 0,
          label: '根节点'
        })
         
          const parents = getListParents(_.cloneDeep(groups), id);
          const parentIds = parents.map(p => p.id)
          parentIds.unshift(0); //如果父节点是0，默认是没有的，需要手动补充一个，让他找到
          this.selectedKeys = parentIds
        
          console.log("selectedKeys的值：" + this.selectedKeys)
          this.editState = true

        }else
        {
         var tempData    = await this.$postRequest(this.$sysPermissionManagerUrl, {
          queryString: '',
        })
        const groups = tempData
        this.groupTree = listToTree(_.cloneDeep(groups), {
          id: 0,
          parentId: 0,
          label: '根节点'
        })

        }
        ++this.cascaderKey
        this.addOrEditDialogVisible = true
      },
      //新增和编辑框中的父级分类 改变触发事件
      parentKeyChange() {
         ++this.cascaderKey
        console.log(this.selectedKeys)
        if (this.selectedKeys.length > 0) {
          this.addOrEditForm.parentId = this.selectedKeys[this.selectedKeys.length - 1]
        } else {
          this.addOrEditForm.parentId = 0
        }
      },

    },
  }

</script>
<style lang="less" scoped>
</style>
