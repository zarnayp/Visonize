using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Visonize.Viewer.Domain.Entities;
using Visonize.Viewer.Domain.Interfaces;


namespace Visonize.Viewer.Domain.Infrastructure
{
    // dimensions-only DTO used for updating viewport geometry without changing image
    public struct ViewportDTO
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public bool IsVisible;
    }

    // DTO that carries image reference together with dimensions - used when initially setting viewports with images
    public struct ViewportWithImageDTO
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public UsImage Image;
    }

    // TODO: this is interface for infrastracture since I want to imaging as separate bounded context
    // once viewer bounded context is moved into separate dll this can be moved and public
    public interface IImageViewArea
    {
        // initial set with images
        void SetViewports(int[] indexes, ViewportWithImageDTO[] viewports);

        // update only geometry for existing viewports (no image change)
        void UpdateViewports(int[] indexes, ViewportDTO[] viewports);

        void RemoveViewports(int[] indexes);
    }
}

namespace Visonize.Viewer.Domain.Entities
{
    using Visonize.Viewer.Domain.Infrastructure;
    using static System.Net.Mime.MediaTypeNames;

    internal class ImageWorkspace : IImageWorkspace
    {
        private readonly UsImage?[] images = new UsImage?[4];
        private readonly IImageViewArea area;

        public IReadOnlyList<UsImage?> Images => this.images.AsReadOnly();

        public event EventHandler<UsImage[]> ImagesChanged;
        public event EventHandler<Layout> LayoutChanged;

     //   public event EventHandler<L> Adde;

        private Layout layout = Layout.Review1x1;

        public ImageWorkspace(IImageViewArea area)
        {
            this.area = area;
        }

        public void AddImage(UsImage image, int index)
        {
            if (index < 0 || index >= images.Length)
            {
                return;
            }

            this.images[index] = image;
        }

        public void RemoveImage(UsImage image)
        {
            int index = Array.IndexOf(this.images, image);
            if (index == -1)
            {
                return;
            }
            this.images[index] = null;
            this.area.RemoveViewports(new int[] { index });
            ImagesChanged?.Invoke(this, this.images);
        }

        public void RemoveImage(int index)
        {
            if (index < 0 || index >= images.Length)
            {
                return;
            }
            this.images[index] = null;

            this.area.RemoveViewports(new int[] { index });
            ImagesChanged?.Invoke(this, this.images);
        }



        public void AddImage(UsImage image)
        {
            // Decide behaviour based on current layout
            switch (this.layout)
            {
                case Layout.Review1x1:
                    // single full-screen viewport -> replace index 0
                    this.images[0] = image;

                    var full = new ViewportWithImageDTO
                    {
                        X = 0f,
                        Y = 0f,
                        Width = 1f,
                        Height = 1f,
                        Image = image
                    };

                    this.area.SetViewports(new int[] { 0 }, new ViewportWithImageDTO[] { full });
                    ImagesChanged?.Invoke(this, this.images);
                    break;

                case Layout.Review2x2:

                    // find first empty slot in images[0..3]
                    int slot = -1;
                    for (int i = 0; i < 4; i++)
                    {
                        if (this.images[i] == null)
                        {
                            slot = i;
                            break;
                        }
                    }

                    if (slot == -1)
                    {
                        // no empty slot; ignore add
                        return;
                    }

                    this.images[slot] = image;

                    // position mapping: 0 - top-left, 1 - top-right, 2 - bottom-left, 3 - bottom-right
                    float x = (slot % 2 == 0) ? 0f : 0.5f;
                    float y = (slot / 2 == 0) ? 0f : 0.5f;

                    var small = new ViewportWithImageDTO
                    {
                        X = x,
                        Y = y,
                        Width = 0.5f,
                        Height = 0.5f,
                        Image = image
                    };

                    this.area.SetViewports(new int[] { slot }, new ViewportWithImageDTO[] { small });
                    ImagesChanged?.Invoke(this, this.images);
                    break;

                case Layout.Dual:
                    // two horizontal viewports. Place into first available of 0 or 1.
                    int dualSlot = -1;
                    for (int i = 0; i < 2; i++)
                    {
                        if (this.images[i] == null)
                        {
                            dualSlot = i;
                            break;
                        }
                    }

                    if (dualSlot == -1)
                    {
                        // both occupied - ignore
                        return;
                    }

                    this.images[dualSlot] = image;

                    var dual = new ViewportWithImageDTO
                    {
                        X = dualSlot == 0 ? 0f : 0.5f,
                        Y = 0f,
                        Width = 0.5f,
                        Height = 1f,
                        Image = image
                    };

                    this.area.SetViewports(new int[] { dualSlot }, new ViewportWithImageDTO[] { dual });
                    ImagesChanged?.Invoke(this, this.images);
                    break;

                case Layout.Live:
                default:
                    // treat Live same as Review1x1
                    this.images[0] = image;

                    var live = new ViewportWithImageDTO
                    {
                        X = 0f,
                        Y = 0f,
                        Width = 1f,
                        Height = 1f,
                        Image = image
                    };

                    this.area.SetViewports(new int[] { 0 }, new ViewportWithImageDTO[] { live });
                    ImagesChanged?.Invoke(this, this.images);
                    break;
            }
        }

        public void ChangeLayout(Layout layout)
        {
            // update internal layout
            this.layout = layout;

            // Build list of indices and dimension-only DTOs for existing images
            var indexes = new List<int>();
            var viewports = new List<ViewportDTO>();

            switch (layout)
            {
                case Layout.Live:
                    for (int i = 0; i < 4; i++)
                    {
                        if (this.images[i] != null)
                        {
                            indexes.Add(i);
                            viewports.Add(new ViewportDTO { X = 0f, Y = 0f, Width = 1f, Height = 1f, IsVisible = false });
                        }
                    }
                    break;
                case Layout.Review1x1:
                    for (int i = 0; i < 4; i++)
                    {
                        if (this.images[i] != null)
                        {
                            var isVisible = (i == 0);

                            indexes.Add(i);
                            viewports.Add(new ViewportDTO { X = 0f, Y = 0f, Width = 1f, Height = 1f, IsVisible = isVisible });
                        }
                    }
                    break;

                case Layout.Review2x2:
                    for (int i = 0; i < 4; i++)
                    {
                        if (this.images[i] != null)
                        {
                            float x = (i % 2 == 0) ? 0f : 0.5f;
                            float y = (i / 2 == 0) ? 0f : 0.5f;
                            indexes.Add(i);
                            viewports.Add(new ViewportDTO { X = x, Y = y, Width = 0.5f, Height = 0.5f , IsVisible = true });
                        }
                    }
                    break;

                case Layout.Dual:
                    for (int i = 0; i < 2; i++)
                    {
                        if (this.images[i] != null)
                        {
                            float x = i == 0 ? 0f : 0.5f;
                            indexes.Add(i);
                            viewports.Add(new ViewportDTO { X = x, Y = 0f, Width = 0.5f, Height = 1f });
                        }
                    }
                    break;
            }

            if (indexes.Count > 0)
            {
                this.area.UpdateViewports(indexes.ToArray(), viewports.ToArray());
            }

            this.LayoutChanged?.Invoke(this, layout);
        }

        
    }
}
